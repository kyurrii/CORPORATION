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
    }
}
