using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using System.Threading;

namespace CORPORATION
{
    class PLANT
    {

        public async void CheckOrdersList(object source, ElapsedEventArgs e)
        {
            int oldestInProdOrdID;
            var cdc = new CorporationDataContext();

            
            var nomberInvNotIssProdOrd = cdc.ProductOrders.Count(s => s.Status == "shiped" && s.InvoiceIssued == "no");
            var invNotIssProdOrd = cdc.ProductOrders.Where(s => s.Status == "shiped" && s.InvoiceIssued == "no").OrderBy(s => s.OrderDate).FirstOrDefault();

            var oldestInProdOrd = cdc.ProductOrders.Where(s => s.Status == "inproduction").OrderBy(s => s.OrderDate).FirstOrDefault();
            var nomberOrdersInProduction = cdc.ProductOrders.Count(s => s.Status == "inproduction");

            if (invNotIssProdOrd!=null)
            {
                await Task.Run( ()=> IssuePlantInvoice(invNotIssProdOrd.MProdOrderID));

                await Task.Run(() => IssuePayment(invNotIssProdOrd.MProdOrderID));

                try
                {
                     invNotIssProdOrd.InvoiceIssued = "yes";
                     cdc.SubmitChanges();
                }
                catch
                {

                }

            }


            if (nomberOrdersInProduction < 3)
            {
                NextOpenOrderToProduction();
            }
            try
            {  
                 oldestInProdOrdID = oldestInProdOrd.MProdOrderID;
            }
            catch { }
            finally
            {

                if (oldestInProdOrd != null)
                {

                    await Task.Run(() => Produce(oldestInProdOrd.MProdOrderID));

                }
            }

        }

        public async void NextOpenOrderToProduction()
        {
            var cdc = new CorporationDataContext();
            int oldestOpOrdID;
            var oldestOpenOrd = cdc.ProductOrders.Where(s => s.Status == "open").OrderBy(s => s.OrderDate).FirstOrDefault();

            try
            {
               
                oldestOpOrdID = oldestOpenOrd.MProdOrderID;
                var orderToProcess = cdc.ProductOrders.Where(s=>s.MProdOrderID== oldestOpOrdID);

                foreach (ProductOrder ord in orderToProcess)
                {
                    ord.Status = "inproduction";
                }

                cdc.SubmitChanges();

            }
            catch (Exception e)
            {

            }

          

          if (oldestOpenOrd!=null)
            {
                await Task.Run(() => Produce(oldestOpenOrd.MProdOrderID));
            }

        }


        public  void Produce(int ordId)
        {
            var cdc = new CorporationDataContext();
            Random rand = new Random();

            int tpro = rand.Next(30000);

            Thread.Sleep(tpro);
            try
            {
                var orderInProcess = cdc.ProductOrders.Where(s => s.MProdOrderID == ordId);

                foreach (ProductOrder ord in orderInProcess)
                {
                    ord.Status = "onstock";

                    cdc.SubmitChanges();
                }


                Thread.Sleep(rand.Next(10000));

                foreach (ProductOrder ord in orderInProcess)
                {

                    ord.Status = "shiped";
                    cdc.SubmitChanges();
                }

            }
            catch (Exception e)
            {

            }
           

        }

        public void IssuePlantInvoice(int ordId)
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
            catch (Exception e)
            {

            }


            
        }


        public void IssuePayment(int ordId)
        {
            var cdc = new CorporationDataContext();


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
                    catch (Exception e)
            {

            }

        }


    }
}
