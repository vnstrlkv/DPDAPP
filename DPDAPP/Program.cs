using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using DPDAPP.DPDGEO;
using DPDAPP.DPDcalcPROM;
using System.IO;
using System.ServiceModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.IO;
using System.Globalization;

namespace HttpClientSample
{

   public class CityDelivery
    {
       public string cityName { get; set; }
        public double weight { get; set; }
        public double volume { get; set; }
       public double Cost { get; set; }
       public int Days { get; set; }
        public string excep { get; set; }
    }
 

    class Program
    {

        static void Main()
        {
           // BasicHttpBinding binding = new BasicHttpBinding();
           // binding.m
            // Use double the default value
          //  binding.MaxReceivedMessageSize = 65536 * 2;
            DPDGeography2Client client = new DPDGeography2Client();
            dpdCitiesCashPayRequest req = new dpdCitiesCashPayRequest();
            ConsoleApp2.DPDGEO.auth tmp = new ConsoleApp2.DPDGEO.auth();
            tmp.clientKey = "A2EA141B4D6C910D747E0678A3A151937B555A48";
            tmp.clientNumber = 1009005163;
             req.auth = tmp;
             req.countryCode = "RU";

            serviceCostRequest req2 = new serviceCostRequest();

            ConsoleApp2.DPDcalcPROM.auth tmp2 = new ConsoleApp2.DPDcalcPROM.auth();
            tmp2.clientKey = "A2EA141B4D6C910D747E0678A3A151937B555A48";
            tmp2.clientNumber = 1009005163;
            req2.auth = tmp2;
            ConsoleApp2.DPDGEO.city[] cities=client.getCitiesCashPay(req);











            DPDCalculatorClient client2 = new DPDCalculatorClient();
          
            cityRequest pic = new cityRequest();

            foreach (ConsoleApp2.DPDGEO.city c in cities)
            {
                if (c.cityName == "Новосибирск")
                {
                    pic.cityIdSpecified = c.cityIdSpecified;
                    pic.countryCode = c.countryCode;
                    pic.cityName = c.cityName;
                    pic.cityId = c.cityId;
                    pic.regionCode = c.regionCode;
                    //pic.index = c.indexMin;
                    pic.regionCodeSpecified = c.regionCodeSpecified;
                }
            }
            req2.pickup = pic;
            string costs="";

            List<CityDelivery> delyveryCity = GetCitiesDelivery();
            int i = 0;
            foreach (CityDelivery DelCity in delyveryCity)
            {
                i++;
                serviceCostRequest request = new serviceCostRequest();

                request = GetserviceCostRequest(req2, DelCity);

                cityRequest del = new cityRequest();
                request.delivery = GetCityRequest(DelCity, del, cities);
                Console.WriteLine(i + ". Обрабатыется город: " + DelCity.cityName + "\n");
                //request.pickup = pic;
            //    if (request.delivery.cityId != 0)
             //      if (request.weight!=0 || request.volume!=0)
                {
                        try
                        {
                            serviceCost[] cost = client2.getServiceCost2(request);
                            DelCity.Cost = cost[0].cost;
                            DelCity.Days = cost[0].days;
                        }
                        catch (Exception e)
                        {
                            DelCity.excep = e.Message;
                        }

                  
                }

              
                
                costs += DelCity.cityName + ";" + DelCity.Cost + ";" + DelCity.Days + ";"+DelCity.excep+"\n";
            }






            SaveToFile("test.csv", costs);
            Console.WriteLine("Готово. Обработано "+i+" городов");
            Console.ReadLine();
            

        }

        static  void SaveToFile(string fileName, string textToSave)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
            {
                sw.Write(textToSave);
            }
        }

        public static serviceCostRequest GetserviceCostRequest (serviceCostRequest request, CityDelivery city)
        {
            request.selfPickup = false;
            request.selfDelivery = false;
            request.weight = city.weight;
            request.volume = city.volume;
            request.serviceCode = "ECN";
            return request;
        }


       public static cityRequest GetCityRequest(CityDelivery city, cityRequest request,  ConsoleApp2.DPDGEO.city[] cities)
        {

            foreach (ConsoleApp2.DPDGEO.city c in cities)
            {
                
                if (c.cityName.ToLower() == city.cityName.ToLower())
                {
                    request.regionCode = c.regionCode;
                    request.cityIdSpecified = c.cityIdSpecified;
                    request.countryCode = c.countryCode;
                    request.cityName = c.cityName;
                    request.cityId = c.cityId;
                    request.regionCodeSpecified = c.regionCodeSpecified;
                }

            }
            
            return request;
        }




        //полчение списка доставок

        private static DataTable WorksheetToDataTable(ExcelWorksheet oSheet)
        {
            int totalRows = oSheet.Dimension.End.Row;
            int totalCols = oSheet.Dimension.End.Column;
            DataTable dt = new DataTable(oSheet.Name);
            DataRow dr = null;
            for (int i = 1; i <= totalRows; i++)
            {
                if (i > 1) dr = dt.Rows.Add();
                for (int j = 1; j <= totalCols; j++)
                {
                    if (i == 1)
                        dt.Columns.Add(oSheet.Cells[i, j].Value.ToString());
                    else
                        if (oSheet.Cells[i, j].Value != null)
                        dr[j - 1] = oSheet.Cells[i, j].Value.ToString();
                }
            }
            return dt;
        }

        public static double GetDouble(string value, double defaultValue)
        {
            double result;

            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }

        public static List <CityDelivery> GetCitiesDelivery()
        {

            FileInfo newFile = new FileInfo("input.xlsx");
            ExcelPackage package = new ExcelPackage(newFile);
            ExcelWorksheet osheet = package.Workbook.Worksheets[1];
            // Materials = WorksheetToDataTable(osheet);
            DataTable tmpMatDT = WorksheetToDataTable(osheet);
            List<CityDelivery> tmp = new List<CityDelivery>();
           
            //  tmpPrice = new PriceList();
            foreach (DataRow row in tmpMatDT.Rows)
            {
                CityDelivery tmpCity = new CityDelivery();
               // Material tmp = new Material();
                for (int j = 0; j < 3; j++)
                //   if (values[j] != null)
                {
                    switch (j)
                    {
                        case 0:
                            {
                                tmpCity.cityName = row[j].ToString(); break;
                            }
                        case 1:
                            { tmpCity.weight = GetDouble(row[j].ToString(), -1); break; }
                        case 2:
                            { tmpCity.volume = GetDouble(row[j].ToString(), -1); break; }
                      
                    }

                }
                tmp.Add(tmpCity);
            }
            return tmp;
        }


    }
}