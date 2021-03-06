﻿using System;
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

        System.Timers.Timer plantMarketTimer = new System.Timers.Timer();
        System.Timers.Timer transMarketTimer = new System.Timers.Timer();
        System.Timers.Timer fuelMarketTimer = new System.Timers.Timer();

         Random random = new Random();

        MARKET mar = new MARKET();
        PLANT plant = new PLANT();
        BANK bank = new BANK();
        FUELSTATION fuelstation = new FUELSTATION();
        CARRIER carrier = new CARRIER();



        private void btnActStart_Click(object sender, EventArgs e)
        {


            int marPeriod = 30000;
            int plantPeriod = 3000;
            int monitorPeriod = 3000;
            int fuelstPeriod = 3000;
            int carrierPeriod = 3000;


            marketTimer.Interval = marPeriod;
            plantTimer.Interval = plantPeriod;
            mainMonitorTimer.Interval = monitorPeriod;
            fuelstationTimer.Interval = fuelstPeriod;
            carrierTimer.Interval = carrierPeriod;

             marketTimer.Elapsed += new ElapsedEventHandler(MarketActivate);
            plantMarketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceProdOrder);
            transMarketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTransOrder);
            fuelMarketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTankFuelOrder);

            //  marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceProdOrder);
            //  marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTransOrder);
            //  marketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTankFuelOrder);

            mainMonitorTimer.Elapsed += new ElapsedEventHandler(MainMonitorRefresh);
            mainMonitorTimer.Elapsed += new ElapsedEventHandler(bank.checkInvoices);
            mainMonitorTimer.Elapsed += new ElapsedEventHandler(bank.checkPayments);

            plantTimer.Elapsed += new ElapsedEventHandler(plant.CheckOrdersList);

      
            fuelstationTimer.Elapsed += new ElapsedEventHandler(fuelstation.CheckTankOrders);

            carrierTimer.Elapsed += new ElapsedEventHandler(carrier.CheckTrucks);
            carrierTimer.Elapsed += new ElapsedEventHandler(carrier.NextTransOrder);
            

            marketTimer.Start();
            plantTimer.Start();
            mainMonitorTimer.Start();
            fuelstationTimer.Start();
            carrierTimer.Start();

        }
        
        private void MarketActivate(object source, ElapsedEventArgs e)
        {
            plantMarketTimer.Stop();
            transMarketTimer.Stop();
            fuelMarketTimer.Stop();

            int marPlantT = random.Next(7000,8000);
            int marTransT = random.Next(10000, 13000);
            int marFuelT = random.Next(5000, 7000);


            plantMarketTimer.Interval = marPlantT;
            transMarketTimer.Interval = marTransT;
            fuelMarketTimer.Interval = marFuelT;

          //  plantMarketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceProdOrder);
          //  transMarketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTransOrder);
          //  fuelMarketTimer.Elapsed += new ElapsedEventHandler(mar.PlaceTankFuelOrder);


            plantMarketTimer.Start();
            transMarketTimer.Start();
            fuelMarketTimer.Start();


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
            PLANT plant = new PLANT();
            

            var cdc = new CorporationDataContext();


            Action textDisplayAct = () => {
          

                

                label3.Text = plant.OpenProdOrderQty().ToString();
              
                label7.Text = plant.InProdOrdQty().ToString();
                label9.Text = plant.OnstockOrdQty().ToString();
               

                label16.Text = bank.balance.ToString("N0");
                label28.Text = bank.PlantInput.ToString("N0");
                label29.Text = bank.FuelInput.ToString("N0");
                label11.Text = bank.TransInput.ToString("N0");

                label19.Text = tfstation.NomberWaitTankOrd().ToString();
                label21.Text = tfstation.NomberProcessTankOrd().ToString();
                label23.Text = tfstation.MomentalTankFuelAmount().ToString();
                label25.Text = tfstation.FuelReserve().ToString();
                label5.Text = tfstation.TankfuelordersCancelledQty().ToString();

             
                label38.Text = carrier.OpenTransOrdersQty().ToString();
                label34.Text = carrier.InProcessTransOrdersQty().ToString();
                label36.Text = carrier.FreeTrucksCount().ToString();
              



            };

            this.BeginInvoke(textDisplayAct);

        }

        private void MainMonitor_Load(object sender, EventArgs e)
        {

            Action MMDaction = MainMonitorDisplay;
            MMDaction();
        }

        

        private void btnActStop_Click_1(object sender, EventArgs e)
        {
            marketTimer.Stop();
            plantTimer.Stop();
            mainMonitorTimer.Stop();
            fuelstationTimer.Stop();
            carrierTimer.Stop();

            plantMarketTimer.Stop();
            transMarketTimer.Stop();
            fuelMarketTimer.Stop();

            CorporationDataContext cdc = new CorporationDataContext();
          /*
            var Items = cdc.BankTransactions;
            foreach (var item in Items)
            {
                cdc.BankTransactions.DeleteOnSubmit(item);
                cdc.SubmitChanges();
            }   */

          /*  var Itemss = cdc.ProductOrders;
            foreach (var item in Itemss)
            {
                cdc.ProductOrders.DeleteOnSubmit(item);
                cdc.SubmitChanges();
            }  */

         /*   var Itemsss = cdc.TankFuelOrders;
            foreach (var item in Itemsss)
            {
                cdc.TankFuelOrders.DeleteOnSubmit(item);
                cdc.SubmitChanges();
            }
            */

        }


    }
}
