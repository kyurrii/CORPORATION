using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Data.Linq;


namespace CORPORATION
{
    class CARRIER
    {


        public async void NextTransOrder(object source, ElapsedEventArgs e)
        {
            var cdc = new CorporationDataContext();
            
            var oldestOpenTransOrd = cdc.TransOrders.Where(s => s.Status == "open").OrderBy(s => s.Date).FirstOrDefault();
            var nextFreeTruck = cdc.Trucks.Where(s => s.Status == "free").OrderBy(s => s.TruckID).FirstOrDefault();
            var oldestInprocessTransOrd = cdc.TransOrders.Where(s => s.Status == "inprocess").OrderBy(s => s.Date).FirstOrDefault();

           

            if (oldestInprocessTransOrd!=null)
            {
                try
                {
                    int ongoingTruckID = (int)cdc.TruckTrips.Where(s => s.TransOrderID == oldestInprocessTransOrd.TransOrderID).Select(m => m.TruckID).FirstOrDefault();
                  var onTripTruck = cdc.Trucks.Where(s => s.TruckID == ongoingTruckID).FirstOrDefault();

                    oldestInprocessTransOrd.Status = "delivered";
                    onTripTruck.Status = "free";
                    cdc.SubmitChanges();
                }
                catch
                {

                }

            }

            if (oldestOpenTransOrd!=null && nextFreeTruck != null)
            {
                int trOrdID = oldestOpenTransOrd.TransOrderID;
                int truckID = nextFreeTruck.TruckID;


                try
                {
                   

                    await Task.Run(()=>StartNewTrip(trOrdID,truckID));

                    oldestOpenTransOrd.Status = "delivered";
                    nextFreeTruck.Status = "free";

                    cdc.SubmitChanges();
                }
                catch
                {

                }


            }

        }


           public void StartNewTrip(int trordID, int truckID)
           {
            int lastItemID;
            int nextItemID;
            int itemsCount = 0;

            var cdc = new CorporationDataContext();

            var nextTransOrder = cdc.TransOrders.Where(s => s.TransOrderID == trordID).FirstOrDefault();
          
            var nextFreeTruck = cdc.Trucks.Where(s => s.TruckID == truckID).FirstOrDefault();

            int distance = (int)nextTransOrder.Distance;
            int dur = distance*5;

            try
            {
                nextTransOrder.Status = "inprocess";
                nextFreeTruck.Status = "onTrip";
                cdc.SubmitChanges();
            }
            catch
            {

            }

            itemsCount = cdc.TruckTrips.Count();

            if (itemsCount == 0)
            {
                nextItemID = 770001;
            }
            else
            {
                lastItemID = cdc.TruckTrips.OrderByDescending(s => s.TruckTripID).Select(s => s.TruckTripID).First();
                nextItemID = lastItemID + 1;
            }


            try
            {
                cdc.TruckTrips.InsertOnSubmit(

                new TruckTrip
                {
                    TruckTripID = nextItemID,
                    TransOrderID = trordID,
                    TruckID = truckID,
                    Date = DateTime.Now

                }
                         );
                cdc.SubmitChanges();

            }
            catch { }

            finally
            {

              
                Thread.Sleep(dur);



            }




           }


        public async void CheckTrucks(object source, ElapsedEventArgs e)
        {
            int itemsCount = 0;
            BANK bank = new BANK();
            int plantruckqty = 10;
            decimal truckprice = 100000;

            var cdc = new CorporationDataContext();
            itemsCount = cdc.Trucks.Count();

            if (itemsCount < plantruckqty && bank.balance > truckprice)
            {
               await Task.Run(()=> PurchaseTruck());
            }


        }


        public void PurchaseTruck()
        {
            int lastItemID;
            int nextItemID;
            int itemsCount = 0;

            var cdc = new CorporationDataContext();


            itemsCount = cdc.Trucks.Count();

            if (itemsCount == 0)
            {
                nextItemID = 990001;
            }
            else
            {
                lastItemID = cdc.Trucks.OrderByDescending(s => s.TruckID).Select(s => s.TruckID).First();
                nextItemID = lastItemID + 1;
            }


            try
            {
                cdc.Trucks.InsertOnSubmit(

                new Truck
                {
                    TruckID = nextItemID,
                    Status = "free"
                    
                }
                         );
                cdc.SubmitChanges();

            }
            catch { }
        }





        public int TrucksCount()
        {
            var cdc = new CorporationDataContext();
            int nomberTrucks = cdc.Trucks.Count();
            return nomberTrucks;
        }

        public int FreeTrucksCount()
        {
            var cdc = new CorporationDataContext();

            int freeTrucksnomber = cdc.Trucks.Where(s => s.Status == "free").Count();
            return freeTrucksnomber;
        }

        public int OpenTransOrdersQty()
        {
            var cdc = new CorporationDataContext();
            int openTransOrdersQty = cdc.TransOrders.Where(s => s.Status == "open").Count();
            return openTransOrdersQty;
        }

        public int InProcessTransOrdersQty()
        {
            var cdc = new CorporationDataContext();

            int inProcessTransOrdersQty= cdc.TransOrders.Where(s => s.Status == "inprocess").Count();
            return inProcessTransOrdersQty;

        }

    }
}
