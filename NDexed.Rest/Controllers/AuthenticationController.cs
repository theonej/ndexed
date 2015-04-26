using System.Web.Mvc;

namespace NDexed.Rest.Controllers
{
    public class AuthenticationController : Controller
    {
        //
        // GET: /Authentication/

        public ActionResult Index()
        {
            var returnUrl = "";
            if (Request.UrlReferrer != null)
            {
                returnUrl = Request.UrlReferrer.AbsoluteUri;
            }

            return View();
        }

    }
}
