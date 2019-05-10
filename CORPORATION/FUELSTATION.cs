using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using System.Data.Linq;

namespace CORPORATION
{
    class FUELSTATION
    {

        decimal FuelReserveLowerLimit = 2000;
        decimal FuelReserveUpperLimit = 10000;
        decimal fuelPrice = 10;
        int FuelStationPostsNumber = 8;
    

        public async void CheckTankOrders(object source, ElapsedEventArgs e)
        {
            

        var cdc = new CorporationDataContext();
            int tankAmount = 0;

            var nomberWaitTankOrd = cdc.TankFuelOrders.Count(s => s.Status == "waiting");
            var nomberProcessTankOrd = cdc.TankFuelOrders.Count(s => s.Status == "inprocess");
            var momentalTankFuelAmount = cdc.TankFuelOrders.Where(s => s.Status == "inprocess").Sum(s=>s.TankFuelOrderAmount);

            var  waitingTankOrd = cdc.TankFuelOrders.Where(s => s.Status == "waiting").OrderBy(s => s.Date).FirstOrDefault();
         

            if (waitingTankOrd != null)
            {
                 tankAmount = (int)waitingTankOrd.TankFuelOrderAmount;
            }

                if (FuelReserve()<= FuelReserveLowerLimit)
                {
                     await Task.Run(() => FuelPurchase());
                }

                if (nomberProcessTankOrd < FuelStationPostsNumber && waitingTankOrd != null && FuelReserve()>=tankAmount)
                {
                    try
                    {
                       int oldestTankOrdID = waitingTankOrd.TankFuelOrderID;
                    

                        await Task.Run(()=>NextVehicleToTank(oldestTankOrdID));

                        await Task.Run(() => TankVehicle(oldestTankOrdID));

                     }
                    catch (Exception ex)
                    {
                      // MessageBox.Show("Exception: " + ex.Message);
 
                    }



                }
                else if ( waitingTankOrd != null && FuelReserve() < tankAmount)
                {

                  Thread.Sleep(2000);
                   try
                   {
                       waitingTankOrd.Status = "canceled";
                       cdc.SubmitChanges();
                   }
                   catch
                   {

                   }
                }

            var inprocTankOrd = cdc.TankFuelOrders.Where(s => s.Status == "inprocess").OrderBy(s => s.Date).FirstOrDefault();
            if (inprocTankOrd != null && FuelReserve() > 0)
            {
                try
                {
                    int inprocTankOrdID = inprocTankOrd.TankFuelOrderID;

                    await Task.Run(() => TankVehicle(inprocTankOrdID));

                }
                catch (Exception ex)
                {
                  //  MessageBox.Show("Exception: " + ex.Message);

                }

            }


        }

        private static void NextVehicleToTank(int ordID)
        {
            var cdc = new CorporationDataContext();
            try { 
                  var waitingTankOrd = cdc.TankFuelOrders.Where(s => s.TankFuelOrderID == ordID);

                   foreach(TankFuelOrder fuelOrder in waitingTankOrd)
                   {
                      fuelOrder.Status = "inprocess";
                     cdc.SubmitChanges();
                   }
                     
                 }
            catch (Exception ex)
            {
              //  MessageBox.Show("Exception: " + ex.Message);

            }

        }

        private static void TankVehicle(int tankOrdId)
        {
            var cdc = new CorporationDataContext();

            var waitingTankOrd = cdc.TankFuelOrders.Where(s => s.TankFuelOrderID == tankOrdId);

            
            foreach (TankFuelOrder ord in waitingTankOrd)
            {
                
                int tankTime =( ord.TankFuelOrderAmount ).GetValueOrDefault()*100;
                Thread.Sleep(tankTime);

                ord.Status = "tanked";
                try
                {
                    cdc.SubmitChanges();
                }
                catch
                {
                }
            }

            
        }



        public  decimal FuelReserve()
        {
           decimal fuelreserve = 0;
            decimal FuelSoldAmount=0;
            decimal TotalFuelPurchasedAmount=0;

            var cdc = new CorporationDataContext();
            var TankFuelInvoicesList = cdc.TankFuelInvoices.Join(cdc.TankFuelOrders,
                                   inv => inv.TankFuelOrderID,
                                   ord => ord.TankFuelOrderID,
                                   (inv, ord) => new
                                   {
                                       TankFuelInvID = inv.TankFuelInvoiceID,
                                       InvoiceValue = ord.TankFuelOrderValue,
                                       InvoiceAmount=ord.TankFuelOrderAmount,
                                       InvoiceStatus = inv.Status,
                                       TankFuelInvDate = inv.Date
                                   
                                   }).ToList();

            decimal soldFuel = Convert.ToDecimal(cdc.TankFuelOrders.Where(s=>s.Status=="tanked").Sum(s=>s.TankFuelOrderAmount));

            var FuelPurchasesPaymentList = cdc.TankFuelPayments;


            int fuelpurchasesCount = cdc.TankFuelPayments.Count();

            if (fuelpurchasesCount == 0)
            {
                 TotalFuelPurchasedAmount = 0;
            }
            else
            {
                TotalFuelPurchasedAmount = Convert.ToDecimal(FuelPurchasesPaymentList.Sum(s => s.FuelPaymentAmount));
            }



               FuelSoldAmount = soldFuel;

             fuelreserve = TotalFuelPurchasedAmount - FuelSoldAmount;

            return fuelreserve;
        }


        public void FuelPurchase()
        {
            decimal fr = FuelReserve();
            decimal amountToPurchase = (FuelReserveUpperLimit - fr);

            var cdc = new CorporationDataContext();
            BANK bank = new BANK();

            if (bank.balance>= amountToPurchase*fuelPrice)
            {

           

            int lastItemID=0;
            int nextItemID=0;
            int itemsCount = 0;

            itemsCount = cdc.TankFuelPayments.Count();

            if (itemsCount == 0)
            {
                nextItemID = 660001;
            }
            else
            {
                lastItemID = cdc.TankFuelPayments.OrderByDescending(s => s.FuelPaymentID).Select(s => s.FuelPaymentID).First();
                nextItemID = lastItemID + 1;
            }

            try
            {

                cdc.TankFuelPayments.InsertOnSubmit(

               new TankFuelPayment
               {
                   FuelPaymentID = nextItemID,
                   FuelPaymentAmount = Convert.ToInt32(amountToPurchase),
                   FuelPaymentValue = amountToPurchase * fuelPrice,

                   Status = "requested",
                   Date=DateTime.Now

               }
                   );
                cdc.SubmitChanges();

            }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);

                }
            }

        }



        public  int NomberWaitTankOrd()
        { 
            
            var cdc = new CorporationDataContext();
            int nomberWaitTankOrd = cdc.TankFuelOrders.Count(s => s.Status == "waiting");
            return nomberWaitTankOrd;
        }
       
        public int NomberProcessTankOrd()
        {
            var cdc = new CorporationDataContext();
            int nomberProcessTankOrd = cdc.TankFuelOrders.Count(s => s.Status == "inprocess");
            return nomberProcessTankOrd;
        }


        public  decimal MomentalTankFuelAmount()
        {
            var cdc = new CorporationDataContext();
            decimal momentalTankFuelAmount =Convert.ToDecimal( cdc.TankFuelOrders.Where(s => s.Status == "inprocess").Sum(s => s.TankFuelOrderAmount));
            return momentalTankFuelAmount;

        }


       public  int TankfuelordersCancelledQty()
        {
            var cdc = new CorporationDataContext();
            int nomberProcessTankOrd = cdc.TankFuelOrders.Count(s => s.Status == "canceled");
            return nomberProcessTankOrd;
        }



    }
}
