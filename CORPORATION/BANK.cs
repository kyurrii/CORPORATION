using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace CORPORATION
{
    class BANK
    {
        public decimal balance = BalanceTuple().Item1;

        public decimal PlantInput = BalanceTuple().Item2;
        public decimal FuelInput = BalanceTuple().Item3;
        public decimal TransInput = BalanceTuple().Item4;


        public async void checkInvoices(object source, ElapsedEventArgs e)
        {
            var cdc = new CorporationDataContext();
           


            try
            {

                var PlantInvoicesList = cdc.PlantInvoices.Join(cdc.ProductOrders,
                                         inv => inv.MProdOrderID,
                                         ord => ord.MProdOrderID,
                                         (inv, ord) => new
                                         {
                                             PlantInvID = inv.PlantInviceID,
                                             InvoiceValue = ord.ProdOrderValue,
                                             InvoiceStatus = inv.Status,
                                             Date = ord.OrderDate,
                                             PaymentTerm = ord.PaymentTerm

                                         }).ToList();

                var oldestPendInvoice = PlantInvoicesList.Where(s => s.InvoiceStatus == "issued").OrderBy(s => s.Date).FirstOrDefault();


                decimal PlantInvoicedTotalAmount = Convert.ToDecimal(PlantInvoicesList.Sum(s => s.InvoiceValue));

                if (oldestPendInvoice != null)
                {
                    int invID = oldestPendInvoice.PlantInvID;
                    var invToProcess = cdc.PlantInvoices.Where(s => s.PlantInviceID == invID);

                    int invPayTerm = (int)oldestPendInvoice.PaymentTerm;

                    Thread.Sleep(invPayTerm*100);

                    await Task.Run(() => createTransaction(invID, null, null, null));

                    foreach (PlantInvoice inv in invToProcess)
                    {
                        inv.Status = "paid";
                    }
                    cdc.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);

            }
        }


        public async void checkPayments(object source, ElapsedEventArgs e)
        {
            var cdc = new CorporationDataContext();

            try
            {
                var PaymentsList = cdc.Payments.Join(cdc.ProductOrders,
                                     pay => pay.MProdOrderID,
                                     ord => ord.MProdOrderID,
                                     (pay, ord) => new
                                     {
                                         CorpPaymID = pay.PaymentID,
                                         PaymentAmount = (ord.ProdOrderValue) * 7 / 10,
                                         PaymentStatus = pay.Status,
                                         Date = ord.OrderDate
                                     });

                var oldestPendPayment = PaymentsList.Where(s => s.PaymentStatus == "requested").OrderBy(s => s.Date).FirstOrDefault();
               
                if (oldestPendPayment != null)
                {
                    int payID = oldestPendPayment.CorpPaymID;
                    var payToProcess = cdc.Payments.Where(s => s.PaymentID == payID);

                    decimal payamt = Convert.ToDecimal(oldestPendPayment.PaymentAmount);

                    decimal currBalance = balance;
                    
                    if(currBalance - payamt >= 0)
                    {

                        await Task.Run(() => createTransaction(null, payID, null, null));

                        foreach (Payment pay in payToProcess)
                        {
                            pay.Status = "paid";
                        }
                        cdc.SubmitChanges();

                    }

                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);

            }

        }
        

        public void createTransaction(int? pinvID,int? payID,int? transID,int? fuelinvID)
        {
            var cdc = new CorporationDataContext();


            int lastItemID;
            int nextItemID;
            int itemsCount = 0;

            itemsCount = cdc.BankTransactions.Count();

            if (itemsCount == 0)
            {
                nextItemID = 440001;
            }
            else
            {
                lastItemID = cdc.BankTransactions.OrderByDescending(s => s.TransactionID).Select(s => s.TransactionID).First();
                nextItemID = lastItemID + 1;
            }

            try
            {

                cdc.BankTransactions.InsertOnSubmit(

               new BankTransaction
               {
                   TransactionID = nextItemID,
                   PlantInvoiceID = pinvID,
                   PaymentID = payID,
                   TransInvoiceID = transID,
                   TankFuelInvoiceID = fuelinvID,
                   Status = "pending",
                   Confirmed = "no",
                   Date = DateTime.Now

               }
                   );
                cdc.SubmitChanges();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);

            }
        }

       


       public  static Tuple <decimal, decimal, decimal,decimal> BalanceTuple()
        {

            decimal balance = 0;
            decimal PlantInput = 0;
            decimal FuelInput = 0;
            decimal TransInput = 0;

            var cdc = new CorporationDataContext();

            try
            {
                var PlantInvoicesList = cdc.PlantInvoices.Join(cdc.ProductOrders,
                                       inv => inv.MProdOrderID,
                                       ord => ord.MProdOrderID,
                                       (inv, ord) => new
                                       {
                                           PlantInvID = inv.PlantInviceID,
                                           InvoiceValue = ord.ProdOrderValue,
                                           InvoiceStatus = inv.Status,
                                           PlantInvDate = ord.OrderDate,
                                           PlantInvPayTerm = ord.PaymentTerm
                                       }).ToList();



                var PaymentsList = from pay in cdc.Payments
                                   where pay.Status == "paid"
                                   join ord in cdc.ProductOrders
                                   on pay.MProdOrderID equals ord.MProdOrderID
                                   select new
                                   {
                                       PaymID = pay.PaymentID,
                                       PaymentAmount = (ord.ProdOrderValue) * 7 / 10,
                                       PaymentStatus = pay.Status
                                   };



                decimal PlantPaymentsValue = Convert.ToDecimal(PaymentsList.Sum(s => s.PaymentAmount));


                decimal PlantInvoicedValue = Convert.ToDecimal(PlantInvoicesList.Sum(s => s.InvoiceValue));

                decimal FuelSoldValue = Convert.ToDecimal(cdc.TankFuelOrders.Where(s => s.Status == "tanked").Sum(s => s.TankFuelOrderValue));

                decimal FuelPurchasedValue = Convert.ToDecimal(cdc.TankFuelPayments.Sum(s => s.FuelPaymentValue));

                decimal TransSoldValue = Convert.ToDecimal(cdc.TransOrders.Where(s => s.Status == "delivered").Sum(s => s.OrderValue));

                decimal TransPaymentsValue = TransSoldValue * 85 / 100;

                decimal InvoicedTotalValue = PlantInvoicedValue + FuelSoldValue + TransSoldValue;
                decimal PaymentsTotalValue = PlantPaymentsValue + FuelPurchasedValue + TransPaymentsValue;
           
         
           

                   balance = InvoicedTotalValue - PaymentsTotalValue;

           
                   FuelInput = (FuelSoldValue - FuelPurchasedValue) / balance*100;
                   PlantInput = (PlantInvoicedValue - PlantPaymentsValue) / balance*100;
                   TransInput = (TransSoldValue - TransPaymentsValue) / balance*100;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);

            }

            return Tuple.Create(balance, PlantInput, FuelInput, TransInput);
        }

    }
}
