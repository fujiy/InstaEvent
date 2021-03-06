﻿using Instaeventos.Core;
using Instaeventos.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Instaeventos.Web.Controllers
{
    public partial class EventController : Controller
    {
        public virtual ActionResult Index(int id)
        {
            using (InstaeventosContext context = new InstaeventosContext())
            {
                var currentEvent = context.Events.Find(id);

                context.SaveChanges();

                return View(id);
            }
        }

        public virtual ActionResult Configure(int? id)
        {
            using (InstaeventosContext context = new InstaeventosContext())
            {
                var viewModel = id.HasValue ?
                    context.Events.Where(x => x.Id == id.Value).Select(x => new EventConfigureSave { Name = x.Name, HashTag = x.HashTag, AutomaticApproval = x.AutomaticApproval }).Single() :
                    new EventConfigureSave();

                return View(viewModel);
            }
        }

        [HttpPost]
        public virtual ActionResult Configure(int? id, EventConfigureSave data)
        {
            if (ModelState.IsValid)
            {
                using (InstaeventosContext context = new InstaeventosContext())
                {
                    var editedEvent = id.HasValue ? context.Events.Find(id.Value) : context.Events.Add(new Event()
                    {
                        User = context.Users.Single(x=>x.UserName == User.Identity.Name),
                        BackgroundUrl = "/Content/Images/1.jpg",
                        CreatedDate = DateTime.Now,
                        Enabled = true,
                        IsPublic = true,
                        StartDate = new DateTime(2013, 1, 1),
                        EndDate = new DateTime(2014, 1, 1),
                        SliderEffect = "normal",
                    });
                    editedEvent.Name = data.Name;
                    editedEvent.HashTag = data.HashTag;
                    editedEvent.AutomaticApproval = data.AutomaticApproval;

                    context.SaveChanges();
                    return RedirectToAction("Approve", new { id = editedEvent.Id });
                }
            }
            return View();
        }

        public virtual async Task<ActionResult> Approve(int id)
        {
            var config = new InstaSharp.InstagramConfig("554dfe9286994bbe98417d8dc7b69a24", "39de8776637b47d2829cd1a4708ae180");

            using (InstaeventosContext context = new InstaeventosContext())
            {
                await new InstagramPhotoFetcher(context, config).ImportNewPhotos(id);

                context.SaveChanges();

                return View(context.InstagramPhotos.Where(x => x.Event.Id == id && x.Approved == false).ToList());
            }
        }

        [HttpPost]
        public virtual ActionResult Approve(int id, int[] selectedItems)
        {
            using (InstaeventosContext context = new InstaeventosContext())
            {
                foreach (var item in context.InstagramPhotos.Where(x => selectedItems.Contains(x.Id)).ToList())
                {
                    item.Approved = true;
                }
                context.SaveChanges();
            }

            return RedirectToAction("Approve");
        }

        public virtual ActionResult Album(int id)
        {
            var config = new InstaSharp.InstagramConfig("554dfe9286994bbe98417d8dc7b69a24", "39de8776637b47d2829cd1a4708ae180");

            using (InstaeventosContext context = new InstaeventosContext())
            {
                return View(context.InstagramPhotos.Where(x => x.Event.Id == id && x.Approved == true).ToList());
            }
        }
    }
}