using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Timers;
using Timer = System.Windows.Forms.Timer;


namespace CORPORATION
{
    public partial class MainMonitor : Form
    {
        public MainMonitor()
        {
            InitializeComponent();
        }

        System.Timers.Timer marketTimer = new System.Timers.Timer();
        System.Timers.Timer plantTimer = new System.Timers.Timer();
        System.Timers.Timer mainMonitorTimer = new System.Timers.Timer();
        System.Timers.Timer fuelstationTimer = new System.Timers.Timer();


        private void button1_Click(object sender, EventArgs e)
        {


            MARKET mar = new MARKET();
            PLANT plant = new PLANT();
            BANK bank = new BANK();
            FUELSTATION fuelstation = new FUELSTATION();


            /* Random random = new Random();

             int  marPeriod = random.Next(10000);
             int  plantPeriod = random.Next(2000);
             int monitorPeriod = random.Next(3000);   */

            int marPeriod = 7000;
            int plantPeriod = 3000;
            int monitorPeriod = 2000;
            int fuelstPeriod = 3000;


            marketTimer.Interval = marPeriod;
            plantTimer.Interval = plantPeriod;
            mainMonitorTimer.Interval = monitorPeriod;
            fuelstationTimer.Interval = fuelstPeriod;

            marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceProdOrder);
            marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTankFuelOrder);

            mainMonitorTimer.Elapsed += new ElapsedEventHandler(MainMonitorRefresh);
            mainMonitorTimer.Elapsed += new ElapsedEventHandler(bank.checkInvoices);
            mainMonitorTimer.Elapsed += new ElapsedEventHandler(bank.checkPayments);

            plantTimer.Elapsed += new ElapsedEventHandler(plant.CheckOrdersList);

            fuelstationTimer.Elapsed += new ElapsedEventHandler(fuelstation.CheckFuelReserve);
            fuelstationTimer.Elapsed += new ElapsedEventHandler(fuelstation.CheckTankOrders);

            marketTimer.Start();
            plantTimer.Start();
            mainMonitorTimer.Start();
            fuelstationTimer.Start();


        }

        public void MainMonitorRefresh(object source, ElapsedEventArgs e)
        {
            MainMonitorDisplay();
        }

        public void MainMonitorDisplay()
        {

            BANK bank = new BANK();
            FUELSTATION tfstation = new FUELSTATION();

            var cdc = new CorporationDataContext();

            int openOrdNum = cdc.ProductOrders.Count(s => s.Status == "open");
            int inprodOrdNum = cdc.ProductOrders.Count(s => s.Status == "inproduction");
            int onStock = cdc.ProductOrders.Count(s => s.Status == "onstock");

            var openOrdList = cdc.ProductOrders.Where(s => s.Status == "open");
            decimal openOrderTotValue = Convert.ToDecimal(openOrdList.Sum(s => s.ProdOrderValue));

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

            var PaymentsList = cdc.Payments.Join(cdc.ProductOrders,
                                 pay => pay.MProdOrderID,
                                 ord => ord.MProdOrderID,
                                 (pay, ord) => new
                                 {
                                     PaymID = pay.PaymentID,
                                     PaymentAmount = (ord.ProdOrderValue) * 7 / 10,
                                     PaymentStatus = pay.Status
                                 }).ToList();
            /*
            var TransactionsList = cdc.BankTransactions.Join(cdc.PlantInvoices,
                                       trans => trans.PlantInvoiceID,
                                       inv => inv.PlantInviceID,
                                       (trans, inv) => new
                                       {
                                           Transaction = trans,
                                           PlantInvoice = inv,
                                       })
                                       .Join(cdc.Payments,
                                          ti => ti.Transaction.PaymentID,
                                          pay => pay.PaymentID,
                                          (ti, pay) => new
                                          {
                                              ti.Transaction,
                                              ti.PlantInvoice,
                                              Payment = pay,
                                          })
                                             .Select(c => new
                                             {
                                                 c.Transaction.TransactionID,
                                                 c.PlantInvoice.PlantInviceID,
                                                 c.Payment.PaymentID
                                             }).ToList();


            var TransactionsList1= from transaction in cdc.BankTransactions
                                   join plntInvoice in cdc.PlantInvoices
                                   on transaction.PlantInvoiceID equals plntInvoice.PlantInviceID
                                   join payment in cdc.Payments
                                   on  transaction.PaymentID equals payment.PaymentID
                                   select new
                                   {
                                       transaction.TransactionID,
                                       plntInvoice.PlantInviceID,
                                       payment.PaymentID
                                   };
                                   */

            /*
             var TransactionsList = cdc.BankTransactions.Join(cdc.PlantInvoices,
                                        trans => trans.PlantInvoiceID,
                                        inv => inv.PlantInviceID,
                                        (trans, inv) => new
                                        {
                                            TransactionID = trans.TransactionID,
                                            PlantInvoiceID = inv.PlantInviceID,
                                            PlantInvoiceValue = (cdc.ProductOrders.Where(s => s.MProdOrderID == inv.MProdOrderID).Select(v => v.ProdOrderValue).FirstOrDefault()),
                                            PaymentID=trans.PaymentID,
                                        })
                                        .Join(cdc.Payments,
                                              transs=>transs.PaymentID, 
                                              pay=>pay.PaymentID,
                                              (transs, pay) => new
                                              {
                                                  TransactionID = transs.TransactionID,
                                                  PlantInvoiceID=transs.PlantInvoiceID,
                                                  PlantInvoiceValue=transs.PlantInvoiceValue,
                                                  PaymentID = transs.PaymentID,
                                                  PaymentAmount= cdc.ProductOrders.Where(s => s.MProdOrderID == pay.MProdOrderID).Select(v => v.ProdOrderValue).FirstOrDefault(),
                                              }
                                        );
                                           */


               decimal TotalPaymentsAmount = Convert.ToDecimal(PaymentsList.Sum(s => s.PaymentAmount));


                decimal PlantInvoicedTotalAmount = Convert.ToDecimal(PlantInvoicesList.Sum(s => s.InvoiceValue));


            Action textDisplayAct = () => {
                dataGridView1.DataSource = cdc.ProductOrders;
                dataGridView2.DataSource = PlantInvoicesList;
                dataGridView3.DataSource = PaymentsList;
                dataGridView4.DataSource = cdc.TankFuelOrders;


                label3.Text = openOrdNum.ToString();
                label5.Text = openOrderTotValue.ToString();
                label7.Text = inprodOrdNum.ToString();
                label9.Text = onStock.ToString();
               // label11.Text = PlantInvoicedTotalAmount.ToString();

                label16.Text = bank.balance.ToString();
                label28.Text = bank.PlantInput.ToString();
                label29.Text = bank.FuelInput.ToString();

                label19.Text = tfstation.nomberWaitTankOrd().ToString();
                label21.Text = tfstation.nomberProcessTankOrd().ToString();
                label23.Text = tfstation.momentalTankFuelAmount().ToString();
                label25.Text = tfstation.fuelReserve.ToString();



            };

            this.BeginInvoke(textDisplayAct);

        }

        private void MainMonitor_Load(object sender, EventArgs e)
        {

            Action MMDaction = MainMonitorDisplay;
            MMDaction();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            marketTimer.Stop();
            plantTimer.Stop();
            mainMonitorTimer.Stop();
            fuelstationTimer.Stop();

        }

    }
}
