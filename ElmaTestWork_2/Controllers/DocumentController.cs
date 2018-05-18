using ElmaTestWork_2.DAL.NHibernate.UnitOfWork;
using ElmaTestWork_2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ElmaTestWork_2.Controllers
{
    public class DocumentController : Controller
    {
        private IStoreNHibernateProviderFactory _store { get; set; }

        public DocumentController(IStoreNHibernateProviderFactory store)
        {
            _store = store;
            _store.Configuration();
        }

        // GET: Document
        public ActionResult Index()
        {
            IQueryable<Document> documents;
            using (_store.Start())
            {
                documents = _store.Current.Session.Query<Document>().ToList().AsQueryable();
            }
            return View(documents.ToList());
        }

        //// GET: Document/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Document document)
        {
            if (Request.Files.Count == 0)
            {
                ModelState.AddModelError("Files", "Необходимо выбрать файл.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView(document);
            }
            try
            {
                List<Document> documents = new List<Document>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        Document newDocument = new Document()
                        {
                            Author = User.Identity.Name,
                            Date = document.Date,
                            OriginalName = fileName,
                            Description = document.Description,
                            FileName = Guid.NewGuid().ToString("D")
                        };
                        documents.Add(newDocument);
                        Debug.WriteLine($"Document FileName {newDocument.FileName}");

                        var path = Path.Combine(Server.MapPath("~/App_Data/FileStorage/"), newDocument.FileName);
                        file.SaveAs(path);
                    }
                }

                if (documents.Count > 0)
                {
                    using (_store.Start())
                    {
                        try
                        {
                            foreach (Document addedDocument in documents)
                                _store.Current.Session.SaveOrUpdate(addedDocument);
                            _store.Current.TransactionalFlush();
                        }
                        catch (Exception ex)
                        {
                            return View("Error", new HandleErrorInfo(ex, "Document", "Create"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Document", "Create"));
            }
            return RedirectToAction("Index");
        }

        // POST: Document/InformaionStore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult InformationStore()
        {
            try
            {
                int fileCount = 0;
                long fileVolume = 0;
                using (_store.Start())
                {
                    IQueryable<Document> documents = _store.Current.Session.Query<Document>();
                    foreach (Document document in documents)
                    {
                        var path = Path.Combine(Server.MapPath("~/App_Data/FileStorage/"), document.FileName);
                        if (System.IO.File.Exists(path))
                        {
                            fileCount++;
                            System.IO.FileInfo file = new System.IO.FileInfo(path);
                            fileVolume += file.Length;
                        }
                    }
                    return Json(new { Result = "OK", Count = fileCount, Volume = BytesToString(fileVolume) });
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB" }; //
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        // POST: Document/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string id)
        {
            if (String.IsNullOrEmpty(id.ToString()))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Document document;
                using (_store.Start())
                {
                    // Delete from database
                    document = _store.Current.Session.Get<Document>(id);
                    if (document == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return Json(new { Result = "Error" });
                    }
                    _store.Current.Session.Delete(document);

                    //Delete file from the file system
                    var path = Path.Combine(Server.MapPath("~/App_Data/FileStorage/"), document.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    _store.Current.TransactionalFlush();
                }
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        // GET: Document/Download
        public FileResult Download(string p, string d)
        {
            return File(Path.Combine(Server.MapPath("~/App_Data/FileStorage/"), p), System.Net.Mime.MediaTypeNames.Application.Octet, d);
        }
    }
}
