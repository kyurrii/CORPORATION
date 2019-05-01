using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using System.Threading;
using System.Windows.Forms;
using System.Data.Linq;


namespace CORPORATION
{
    class PLANT
    {


        private int prodLinesnomber = 4;

        public async void CheckOrdersList(object source, ElapsedEventArgs e)
        {
          
            var cdc = new CorporationDataContext();

            
            var nomberInvNotIssProdOrd = cdc.ProductOrders.Count(s => s.Status == "shiped" && s.InvoiceIssued == "no");
            var invNotIssProdOrd = cdc.ProductOrders.Where(s => s.Status == "shiped" && s.InvoiceIssued == "no").OrderBy(s => s.OrderDate).FirstOrDefault();

            var oldestInProdOrd = cdc.ProductOrders.Where(s => s.Status == "inproduction").OrderBy(s => s.OrderDate).FirstOrDefault();
            var nomberOrdersInProduction = cdc.ProductOrders.Count(s => s.Status == "inproduction");

            var notShipedProdOrd= cdc.ProductOrders.Where(s => s.Status == "onStock").OrderBy(s => s.OrderDate).FirstOrDefault();


            if (notShipedProdOrd!=null)
            {
                int ntordID = notShipedProdOrd.MProdOrderID;

                await Task.Run(() => ShipProdOrd(ntordID));
            }

            if (invNotIssProdOrd!=null)
            {
                await Task.Run(()=> IssuePlantInvoice(invNotIssProdOrd.MProdOrderID));

                await Task.Run(() => IssuePayment(invNotIssProdOrd.MProdOrderID));

                try
                {
                     invNotIssProdOrd.InvoiceIssued = "yes";
                     cdc.SubmitChanges();
                }
                catch (Exception ex)
                {
                  //  MessageBox.Show("Exception: " + ex.Message);

                }

            }


            if (nomberOrdersInProduction < prodLinesnomber)
            {
                NextOpenOrderToProduction();
            }

            if (oldestInProdOrd != null)
            {

                await Task.Run(() => Produce(oldestInProdOrd.MProdOrderID));

            }


        }

        public async void NextOpenOrderToProduction()
        {
            var cdc = new CorporationDataContext();
      
            var oldestOpenOrd = cdc.ProductOrders.Where(s => s.Status == "open").OrderBy(s => s.OrderDate).FirstOrDefault();

            if (oldestOpenOrd != null)
            {
                try
                {
                    oldestOpenOrd.Status = "inproduction";

                    cdc.SubmitChanges();

                    await Task.Run(() => Produce(oldestOpenOrd.MProdOrderID));
                }
                catch (Exception ex)
                {
                    //  MessageBox.Show("Exception: " + ex.Message);

                }
               
                
            }

           
        }


        public async void Produce(int ordId)
        {
            var cdc = new CorporationDataContext();
            Random rand = new Random();

            int tpro = rand.Next(30000);

            Thread.Sleep(tpro);
            try
            {
                var orderInProcess = cdc.ProductOrders.Where(s => s.MProdOrderID == ordId).First();


                orderInProcess.Status= "onstock";
                cdc.SubmitChanges();

                Thread.Sleep(rand.Next(8000));

                await Task.Run(()=> ShipProdOrd(ordId));


              //  orderInProcess.Status = "shiped";
                //cdc.SubmitChanges();

            }
            catch (Exception ex)
            {
               // MessageBox.Show("Exception: " + ex.Message);

            }


        }

        private void ShipProdOrd(int ordId)
        {
            var cdc = new CorporationDataContext();
            try
            {
                var orderInProcess = cdc.ProductOrders.Where(s => s.MProdOrderID == ordId).First();

                orderInProcess.Status = "shiped";
                cdc.SubmitChanges();

            }
            catch (Exception ex)
            {
                // MessageBox.Show("Exception: " + ex.Message);

            }



        }

        private void IssuePlantInvoice(int ordId)
        {
            var cdc = new CorporationDataContext();


            int lastItemID;
            int nextItemID;
            int itemsCount = 0;

            itemsCount = cdc.PlantInvoices.Count();

            if (itemsCount == 0)
            {
                nextItemID = 330001;
            }
            else
            {
                lastItemID = cdc.PlantInvoices.OrderByDescending(s => s.PlantInviceID).Select(s => s.PlantInviceID).First();
                nextItemID = lastItemID + 1;
            }

            try
            {
               
                    cdc.PlantInvoices.InsertOnSubmit(

                   new PlantInvoice
                   {
                       PlantInviceID = nextItemID,
                       MProdOrderID = ordId,
                       Status = "issued",
                   }
                       );
                    cdc.SubmitChanges();


            }
            catch (Exception ex)
            {
              //  MessageBox.Show("Exception: " + ex.Message);

            }



        }


        public void IssuePayment(int ordId)
        {
            var cdc = new CorporationDataContext();
            BANK bank = new BANK();

           decimal balance = bank.balance;


            int lastItemID;
            int nextItemID;
            int itemsCount = 0;

            itemsCount = cdc.Payments.Count();

            if (itemsCount == 0)
            {
                nextItemID = 220001;
            }
            else
            {
                lastItemID = cdc.Payments.OrderByDescending(s => s.PaymentID).Select(s => s.PaymentID).First();
                nextItemID = lastItemID + 1;
            }

            try
            {

                cdc.Payments.InsertOnSubmit(

               new Payment
               {
                   PaymentID = nextItemID,
                   MProdOrderID = ordId,
                   Status = "requested",
                   TankFuelOrderID = null

               }
                   );
                cdc.SubmitChanges();

            }
            catch (Exception ex)
            {
               // MessageBox.Show("Exception: " + ex.Message);

            }

        }
       

        public int OpenProdOrderQty()
        {
            var cdc = new CorporationDataContext();
            int openOrdNum = cdc.ProductOrders.Count(s => s.Status == "open");
            return openOrdNum;
        }

        public int InProdOrdQty()
        {
            var cdc = new CorporationDataContext();
            int inprodOrdNum = cdc.ProductOrders.Count(s => s.Status == "inproduction");
            return inprodOrdNum;
        }
             
        public int OnstockOrdQty()
        {
            var cdc = new CorporationDataContext();
            int onStock = cdc.ProductOrders.Count(s => s.Status == "onstock");
            return onStock;
        }
       






    }
}
