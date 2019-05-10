using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using System.Collections;

namespace CORPORATION
{
  public  class MARKET
    {
        public void PlaceProdOrder(object source, ElapsedEventArgs e)
        {
            int lastItemID;
            int nextItemID;
            int itemsCount=0;

            var cdc = new CorporationDataContext();


            itemsCount = cdc.ProductOrders.Count();

            if (itemsCount == 0)
            {
                nextItemID = 110001;
            }
            else
            {
                lastItemID = cdc.ProductOrders.OrderByDescending(s => s.MProdOrderID).Select(s => s.MProdOrderID).First();
                nextItemID = lastItemID+1;
            }


            Random random = new Random();
            int orderValue = random.Next(50000);

            List<int>  payTerms = new List<int>() { 0, 30, 60, 90};
            int payIndex = random.Next(4);

            try
            {
                cdc.ProductOrders.InsertOnSubmit(

                new ProductOrder
                {
                    MProdOrderID = nextItemID,
                    ProdOrderValue = orderValue,
                    Status = "open",
                    OrderDate = DateTime.Now,
                    PaymentTerm = payTerms[payIndex],
                    InvoiceIssued = "no"
                }
                         );
                cdc.SubmitChanges();

            }
            catch { }
        }


        public void PlaceTankFuelOrder(object source, ElapsedEventArgs e)
        {
            int lastItemID=0;
            int nextItemID=0;
            int itemsCount = 0;

            var cdc = new CorporationDataContext();


            itemsCount = cdc.TankFuelOrders.Count();

            if (itemsCount == 0)
            {
                nextItemID = 550001;
            }
            else
            {
                lastItemID = cdc.TankFuelOrders.OrderByDescending(s => s.TankFuelOrderID).Select(s => s.TankFuelOrderID).First();
                nextItemID = lastItemID + 1;
            }


            Random random = new Random();
            int orderAmount = random.Next(200);

            decimal fuelPrice = 29;
            decimal orderValue = orderAmount * fuelPrice;

          

            try
            {
                cdc.TankFuelOrders.InsertOnSubmit(

                new TankFuelOrder
                {
                    TankFuelOrderID = nextItemID,
                    TankFuelOrderValue = orderValue,
                    TankFuelOrderAmount=orderAmount,
                    Status = "waiting",
                    Date = DateTime.Now,
                  
                }
                         );
                cdc.SubmitChanges();

            }
            catch { }
        }

        public void PlaceTransOrder(object source, ElapsedEventArgs e)
        {
            int lastItemID=0;
            int nextItemID=0;
            int itemsCount = 0;

            var cdc = new CorporationDataContext();


            itemsCount = cdc.TransOrders.Count();

            if (itemsCount == 0)
            {
                nextItemID = 880001;
            }
            else
            {
                lastItemID = cdc.TransOrders.OrderByDescending(s => s.TransOrderID).Select(s => s.TransOrderID).First();
                nextItemID = lastItemID + 1;
            }


            Random random = new Random();
            int orderValue = random.Next(30000);
            int distance = random.Next(10000);


            try
            {
                cdc.TransOrders.InsertOnSubmit(

                new TransOrder
                {
                    TransOrderID = nextItemID,
                    OrderValue = orderValue,
                    Status = "open",
                    Date = DateTime.Now,
                    Distance = distance,
                    Attribute = null
                }
                         );
                cdc.SubmitChanges();

            }
            catch { }
        }

    }
}
