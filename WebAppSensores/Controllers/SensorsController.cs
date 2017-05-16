using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebAppSensores.Models;

using Microsoft.ServiceBus.Messaging;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace WebAppSensores.Controllers
{
    [Authorize]
    public class SensorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        static string connectionString = "";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        // GET: Sensors
        public ActionResult Index()
        {
            Response.AddHeader("Refresh", "60"); //Refresca la página cada 60 segundos para recuperar los datos nuevos.

            if (Request.Url != Request.UrlReferrer) //No estoy refrescando la página, es el primer acceso. 
            {                                       // Muestro directamente lo que hay en BD, sin ir a recibirlos del EventHub
                return View(db.Sensors.ToList());
            }
            else    //Vengo de la misma página en la que se muestran los datos, 
            {       //bien por el refresco, bien porque he clickado en el enlace "Sensores Cloud" del menú superior

                eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

                var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

                CancellationTokenSource cts = new CancellationTokenSource();

                /*System.Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Exiting...");
                };*/



                //var tasks = new List<Task>();
                foreach (string partition in d2cPartitions)
                {
                    //tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
                    ReceiveMessagesFromDevice(partition, cts.Token);
                }
                //Task.WaitAll(tasks.ToArray());
                return View(db.Sensors.ToList());
            }
        }

        private void ReceiveMessagesFromDevice(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now.AddDays(-1));
            //var eventHubReceiver = eventHubClient.GetConsumerGroup("webapp").CreateReceiver(partition, DateTime.Now.AddDays(-1));
            int i = 0;
            while (i < 4)
            {
                if (ct.IsCancellationRequested) break;
                EventData eventData = eventHubReceiver.Receive();
                //if (eventData == null) continue;

                if (eventData == null) break;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                /*MemoryStream stream1 = new MemoryStream(eventData.GetBytes());
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Sensor));

                stream1.Position = 0;
                Sensor sensor = (Sensor)ser.ReadObject(stream1);*/

                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                Sensor sensor = json_serializer.Deserialize<Sensor>(data);

                db.Sensors.Add(sensor);
                db.SaveChanges();
                i++;

                //Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
                /*Sensor sensor = new Sensor (data.);
                db.Sensors.Add(sensor);
                db.SaveChanges();*/

            }

            eventHubReceiver.Close();

            //return RedirectToAction("Index");


            return;

        }


        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Today);
            //while (true)
            int i = 0;
            while (i < 4)
            {
                if (ct.IsCancellationRequested) break;
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                //if (eventData == null) continue;

                if (eventData == null) break;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                /*MemoryStream stream1 = new MemoryStream(eventData.GetBytes());
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Sensor));

                stream1.Position = 0;
                Sensor sensor = (Sensor)ser.ReadObject(stream1);*/

                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                Sensor sensor = json_serializer.Deserialize<Sensor>(data);

                db.Sensors.Add(sensor);
                db.SaveChanges();
                i++;

                //Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
                /*Sensor sensor = new Sensor (data.);
                db.Sensors.Add(sensor);
                db.SaveChanges();*/

            }

            //return RedirectToAction("Index");


            return;

        }

        /*public ActionResult Index(SensorFilterModel filterModel)
        {
            var business = new SensorBusinessLogic();
            var model = business.GetSensors(filterModel);
            return View(model);
        }*/

        // GET: Sensors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sensor sensor = db.Sensors.Find(id);
            if (sensor == null)
            {
                return HttpNotFound();
            }
            return View(sensor);
        }

        // GET: Sensors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sensors/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DeviceId,Type,Value,TimeStamp")] Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                db.Sensors.Add(sensor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sensor);
        }

        // GET: Sensors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sensor sensor = db.Sensors.Find(id);
            if (sensor == null)
            {
                return HttpNotFound();
            }
            return View(sensor);
        }

        // POST: Sensors/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DeviceId,Type,Value,TimeStamp")] Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sensor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sensor);
        }

        // GET: Sensors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sensor sensor = db.Sensors.Find(id);
            if (sensor == null)
            {
                return HttpNotFound();
            }
            return View(sensor);
        }

        // POST: Sensors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sensor sensor = db.Sensors.Find(id);
            db.Sensors.Remove(sensor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
