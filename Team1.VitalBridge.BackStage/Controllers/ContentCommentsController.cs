using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentCommentsController : Controller
    {
        // GET: ContentCommentsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ContentCommentsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ContentCommentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ContentCommentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ContentCommentsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ContentCommentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ContentCommentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ContentCommentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
