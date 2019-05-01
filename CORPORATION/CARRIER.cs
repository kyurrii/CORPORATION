using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Data.Linq;
using System.Windows.Forms;


namespace CORPORATION
{
    class CARRIER
    {
        
       private  int  plantruckqty = 10;
       

        public async void NextTransOrder(object source, ElapsedEventArgs e)
        {
            var cdc = new CorporationDataContext();

            var oldestOpenTransOrd = cdc.TransOrders.Where(s => s.Status == "open").OrderBy(s => s.Date).FirstOrDefault();
                var nextFreeTruck = cdc.Trucks.Where(s => s.Status == "free").OrderBy(s => s.TruckID).FirstOrDefault();
                var oldestInprocessTransOrd = cdc.TransOrders.Where(s => s.Status == "inprocess").OrderBy(s => s.Date).FirstOrDefault();
            int inprocTransOrdNomber = cdc.TransOrders.Where(s => s.Status == "inprocess").Count();


            


            if ( oldestOpenTransOrd != null && nextFreeTruck != null && inprocTransOrdNomber<= plantruckqty)
            {
                int trOrdID = oldestOpenTransOrd.TransOrderID;
                int truckID = nextFreeTruck.TruckID;

                try
                {

                    await Task.Run(() => StartNewTrip(trOrdID, truckID));
                    await Task.Run(() => Deliver(trOrdID, truckID));
                }
                catch (Exception ex)
                {
                  //  MessageBox.Show("Exception: " + ex.Message);

                }
            }


            if (oldestInprocessTransOrd != null)
            {
                try
                {
                    int ordid = oldestInprocessTransOrd.TransOrderID;
                    int onTripTruckID = (int)cdc.TruckTrips.Where(s => s.TransOrderID == oldestInprocessTransOrd.TransOrderID).Select(m => m.TruckID).FirstOrDefault();
                    var onTripTruck = cdc.Trucks.Where(s => s.TruckID == onTripTruckID && s.Status == "onTrip").FirstOrDefault();

                    await Task.Run(() => Deliver(ordid, onTripTruckID));

                }
                catch (Exception ex)
                {
                    //  MessageBox.Show("Exception: " + ex.Message);

                }

            }

        }


        private void StartNewTrip(int trordID, int truckID)
        {
            int lastItemID=0;
            int nextItemID=0;
            int itemsCount = 0;

              var cdc = new CorporationDataContext();

            var nextTransOrder = cdc.TransOrders.Where(s => s.TransOrderID == trordID).FirstOrDefault();
          
            var nextFreeTruck = cdc.Trucks.Where(s => s.TruckID == truckID).FirstOrDefault();

            Thread.Sleep(1000);

            try
            {
                nextFreeTruck.Status = "onTrip";
                cdc.SubmitChanges();

                nextTransOrder.Status = "inprocess";
                cdc.SubmitChanges();
                

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception: " + ex.Message);

            }

            itemsCount = cdc.TruckTrips.Count();

            if (itemsCount == 0)
            {
                nextItemID = 770001;
            }
            else
            {
                var containsTransOrder = cdc.TruckTrips.Where(s => s.TransOrderID == trordID).First();

                if (containsTransOrder==null)
                {
                    lastItemID = cdc.TruckTrips.OrderByDescending(s => s.TruckTripID).Select(s => s.TruckTripID).First();
                    nextItemID = lastItemID + 1;

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

                    catch (ChangeConflictException e)
                    {


                    }
                
                }

            
               

            }
           




        }

        private  void Deliver(int trordID, int truckID)
        {
                  var cdc = new CorporationDataContext();

                 var nextTransOrder = cdc.TransOrders.Where(s => s.TransOrderID == trordID).FirstOrDefault();
                 var nextTruck = cdc.Trucks.Where(s => s.TruckID == truckID).FirstOrDefault();

                  int distance = (int)nextTransOrder.Distance;
                  int dur = distance * 5;
                  Thread.Sleep(dur);

                try
                 {
                   nextTransOrder.Status = "delivered";
                   cdc.SubmitChanges();

                   nextTruck.Status = "free";
                   cdc.SubmitChanges();
                 }
                 catch (ChangeConflictException e)
                 {
                    
                 }
              

        }
             

        public async void CheckTrucks(object source, ElapsedEventArgs e)
        {

            var cdc = new CorporationDataContext();

            int itemsCount = cdc.Trucks.Count();

            if (itemsCount < plantruckqty )
            {
               await Task.Run(()=> PurchaseTruck());
            }


        }
      /*
        private void TechServiseTruck(int trID)
        {

          Random random = new Random();

          int ff = random.Next(10);

          if (ff >= 7)
          {
            var firstTruck = cdc.Trucks.Where(s => s.Status == "free").OrderBy(s => s.TruckID).FirstOrDefault();

                try
                {
                  firstTruck.Status = "onTechService";
                  cdc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);

                }

                int firstTruckID = firstTruck.TruckID;

        

                var servTruck = cdc.Trucks.Where(s => s.TruckID == trID).FirstOrDefault();

                Thread.Sleep(10000);


               try
               {
                servTruck.Status = "free";
                cdc.SubmitChanges();

               }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);

                }
          }
          
        }    */



        private  void PurchaseTruck()
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

                cdc.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException e)
            {
                foreach (ObjectChangeConflict occ in cdc.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.KeepChanges);
                }
            }
            try
            {
                cdc.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
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

        public int OnServiceTruckQty()
        {
            var cdc = new CorporationDataContext();
            int  onServiceTruckQty = cdc.Trucks.Where(s => s.Status == "onTechService").Count();
            return onServiceTruckQty;


        }
    }
}
