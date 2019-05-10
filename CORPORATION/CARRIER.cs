using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Data.Linq;
using System.Windows.Forms;

using System.Windows.Forms;


namespace CORPORATION
{
    class CARRIER
    {
        
       private  int  plantruckqty = 10;
       private Mutex mutex = new Mutex();

        public async void NextTransOrder(object source, ElapsedEventArgs e)
        {
            using (CorporationDataContext cdc = new CorporationDataContext())
            {



                int inprocTransOrdNomber = cdc.TransOrders.Where(s => s.Status == "inprocess").Count();

               

                var oldestOpenTransOrd = cdc.TransOrders.Where(s => s.Status == "open").OrderBy(s => s.Date).FirstOrDefault();
                var nextFreeTruck = cdc.Trucks.Where(s => s.Status == "free").OrderBy(s => s.TruckID).FirstOrDefault();
                var openTripTruckCheck=cdc.TruckTrips.Where(t=>t.TruckID==nextFreeTruck.TruckID && t.Status=="open");

                if (oldestOpenTransOrd != null && nextFreeTruck != null && inprocTransOrdNomber < plantruckqty)   //&& openTripTruckCheck==null
                {
                    int trOrdID = oldestOpenTransOrd.TransOrderID;
                    int truckID = nextFreeTruck.TruckID;

                    try
                    {

                        await Task.Run(() => StartNewTrip(trOrdID, truckID));
                      //  Thread.Sleep(1500);
                      //  await Task.Run(() => Deliver(trOrdID, truckID));
                    }
                    catch (Exception ex)
                    {
                        //  MessageBox.Show("Exception: " + ex.Message);

                    }
                }
            
                var oldestInprocessTransOrd = cdc.TransOrders.Where(s => s.Status == "inprocess").OrderBy(s => s.Date).FirstOrDefault();

                if (oldestInprocessTransOrd != null )
                {
                    try
                    {
                        int ordid = oldestInprocessTransOrd.TransOrderID;
                        int onTripTruckID = (int)cdc.TruckTrips.Where(s => s.TransOrderID == ordid).Select(m => m.TruckID).FirstOrDefault();
                        var onTripTruck = cdc.Trucks.Where(s => s.TruckID == onTripTruckID ).FirstOrDefault();   //&& s.Status == "onTrip"

                        if (onTripTruck != null)
                        {
                            await Task.Run(() => Deliver(ordid, onTripTruck.TruckID));
                        }
                    }
                    catch (Exception ex)
                    {
                        //  MessageBox.Show("Exception: " + ex.Message);

                    }

                }     

            }
        }


        private void StartNewTrip(int trordID, int truckID)
        {
            int lastItemID = 0;
            int nextItemID = 0;
            int itemsCount = 0;

            using( CorporationDataContext cdc = new CorporationDataContext())
            {

                TransOrder nextTransOrder = cdc.TransOrders.Where(s => s.TransOrderID == trordID).FirstOrDefault();

                Truck nextFreeTruck = cdc.Trucks.Where(s => s.TruckID == truckID).FirstOrDefault();

                Thread.Sleep(1000);

                try
                {

                    nextFreeTruck.Status = "onTrip";
                     cdc.SubmitChanges();

                    nextTransOrder.Attribute = "onWay";
                    cdc.SubmitChanges();

                    nextTransOrder.Status = "inprocess";
                    cdc.SubmitChanges();


                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Exception: " + ex.Message);

                }
                try
                {

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

                    Boolean containsTransOrder = cdc.TruckTrips.Select(s => s.TransOrderID).Contains(trordID);

                    if (containsTransOrder == false)
                    {

                        try
                        {
                            cdc.TruckTrips.InsertOnSubmit(

                            new TruckTrip
                            {
                                TruckTripID = nextItemID,
                                TransOrderID = trordID,
                                TruckID = truckID,
                                Date = DateTime.Now,
                                Status = "open"

                            }
                                     );

                            cdc.SubmitChanges();


                        }

                        catch (ChangeConflictException e)
                        {


                        }


                    }


                }
                catch
                {

                }




            }
        }

        private void Deliver(int trordID, int truckID)
        {
            using (var cdc = new CorporationDataContext())
            {

              //  mutex.WaitOne();

                TransOrder nextTransOrder = cdc.TransOrders.Where(s => s.TransOrderID == trordID).FirstOrDefault();
                Truck nextTruck = cdc.Trucks.Where(s => s.TruckID == truckID).FirstOrDefault();
                TruckTrip currTrip = cdc.TruckTrips.Where(s => s.TransOrderID == trordID).Single();

                int distance = (int)nextTransOrder.Distance;
                int dur = distance * 1;
                Thread.Sleep(dur);

                try
                {
                    nextTransOrder.Status = "complete";

                     cdc.SubmitChanges();

                    currTrip.Status = "complete";

                    cdc.SubmitChanges();

                    nextTruck.Status = "free";

                    cdc.SubmitChanges();


                    if(nextTransOrder.Status!= "complete")
                    {
                        MessageBox.Show( nextTransOrder.Status, "nextTransOrder.Status= ");
                        
                    }

                    if (currTrip.Status != "complete")
                    {
                        MessageBox.Show( currTrip.Status, "currTrip.Status= ");

                    }

                    if (nextTruck.Status!= "free")
                    {
                        MessageBox.Show( nextTruck.Status, "nextTruck.Status= ");

                    }




                }
                catch (ChangeConflictException e)
                {
                    /* foreach (ObjectChangeConflict occ in cdc.ChangeConflicts)
                     {
                         occ.Resolve(RefreshMode.KeepChanges);
                     }  */
                }
                /*   try
                   {

                        cdc.SubmitChanges(ConflictMode.FailOnFirstConflict);
                   }
                   catch (Exception ex)
                   {
                       MessageBox.Show("Exception: " + ex.Message);
                   }*/

               // mutex.ReleaseMutex();
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
            using (CorporationDataContext cdc = new CorporationDataContext())
            {
                int nomberTrucks = cdc.Trucks.Count();
                return nomberTrucks;
            }
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
            int inProcessTransOrdersQty=0;
        /*
            if (cdc.Trucks.Count()== plantruckqty)
            {
                

                 inProcessTransOrdersQty = plantruckqty - FreeTrucksCount();
            }
            else
            {
                inProcessTransOrdersQty = 0;
            }                     */

            inProcessTransOrdersQty = cdc.TransOrders.Where(s => s.Status == "inprocess").Count();
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
