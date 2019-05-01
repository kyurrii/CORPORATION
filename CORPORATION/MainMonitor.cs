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
        System.Timers.Timer carrierTimer = new System.Timers.Timer();

        private void button1_Click(object sender, EventArgs e)
        {


            MARKET mar = new MARKET();
            PLANT plant = new PLANT();
            BANK bank = new BANK();
            FUELSTATION fuelstation = new FUELSTATION();
            CARRIER carrier = new CARRIER();


            /* Random random = new Random();

             int  marPeriod = random.Next(10000);
             int  plantPeriod = random.Next(2000);
             int monitorPeriod = random.Next(3000);   */

            int marPeriod = 5000;
            int plantPeriod = 3000;
            int monitorPeriod = 2000;
            int fuelstPeriod = 3000;
            int carrierPeriod = 3000;


            marketTimer.Interval = marPeriod;
            plantTimer.Interval = plantPeriod;
            mainMonitorTimer.Interval = monitorPeriod;
            fuelstationTimer.Interval = fuelstPeriod;
            carrierTimer.Interval = carrierPeriod;

            marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceProdOrder);
            marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTankFuelOrder);
            marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTransOrder);

            mainMonitorTimer.Elapsed += new ElapsedEventHandler(MainMonitorRefresh);
            mainMonitorTimer.Elapsed += new ElapsedEventHandler(bank.checkInvoices);
            mainMonitorTimer.Elapsed += new ElapsedEventHandler(bank.checkPayments);

            plantTimer.Elapsed += new ElapsedEventHandler(plant.CheckOrdersList);

            fuelstationTimer.Elapsed += new ElapsedEventHandler(fuelstation.CheckFuelReserve);
            fuelstationTimer.Elapsed += new ElapsedEventHandler(fuelstation.CheckTankOrders);

            carrierTimer.Elapsed += new ElapsedEventHandler(carrier.CheckTrucks);
            carrierTimer.Elapsed += new ElapsedEventHandler(carrier.NextTransOrder);
            

            marketTimer.Start();
            plantTimer.Start();
            mainMonitorTimer.Start();
            fuelstationTimer.Start();
            carrierTimer.Start();

        }

        public void MainMonitorRefresh(object source, ElapsedEventArgs e)
        {
            MainMonitorDisplay();
        }

        public void MainMonitorDisplay()
        {

            BANK bank = new BANK();
            FUELSTATION tfstation = new FUELSTATION();
            CARRIER carrier = new CARRIER();

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
          


               decimal TotalPaymentsAmount = Convert.ToDecimal(PaymentsList.Sum(s => s.PaymentAmount));


                decimal PlantInvoicedTotalAmount = Convert.ToDecimal(PlantInvoicesList.Sum(s => s.InvoiceValue));


            Action textDisplayAct = () => {
                dataGridView1.DataSource = cdc.TransOrders;
              //  dataGridView2.DataSource = PlantInvoicesList;
              //  dataGridView3.DataSource = PaymentsList;
                dataGridView4.DataSource = cdc.Trucks;


                label3.Text = openOrdNum.ToString();
                label5.Text = openOrderTotValue.ToString("N2");
                label7.Text = inprodOrdNum.ToString();
                label9.Text = onStock.ToString();
               

                label16.Text = bank.balance.ToString("N2");
                label28.Text = bank.PlantInput.ToString("N");
                label29.Text = bank.FuelInput.ToString("N");
                label11.Text = bank.TransInput.ToString("N");

                label19.Text = tfstation.nomberWaitTankOrd().ToString();
                label21.Text = tfstation.nomberProcessTankOrd().ToString();
                label23.Text = tfstation.momentalTankFuelAmount().ToString();
                label25.Text = tfstation.fuelReserve.ToString();
                label32.Text = carrier.TrucksCount().ToString();
                label38.Text = carrier.OpenTransOrdersQty().ToString();
                label34.Text = carrier.InProcessTransOrdersQty().ToString();
                label36.Text = carrier.FreeTrucksCount().ToString();
                label32.Text = carrier.OnServiceTruckQty().ToString();



            };

            this.BeginInvoke(textDisplayAct);

        }

        private void MainMonitor_Load(object sender, EventArgs e)
        {

            Action MMDaction = MainMonitorDisplay;
            MMDaction();
        }

        

        private void button2_Click_1(object sender, EventArgs e)
        {
            marketTimer.Stop();
            plantTimer.Stop();
            mainMonitorTimer.Stop();
            fuelstationTimer.Stop();
            carrierTimer.Stop();
        }
    }
}
