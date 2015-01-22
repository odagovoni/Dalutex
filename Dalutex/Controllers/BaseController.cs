﻿using Dalutex.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dalutex.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #region ErrorHandling

        protected string GetInnerException(Exception e, string strIn = "")
        {
            strIn = strIn + e.Message;

            if (e.InnerException != null)
                return GetInnerException(e.InnerException, strIn + Environment.NewLine);
            else
                return strIn;
        }

        protected void HandleDbEntityValidationException(DbEntityValidationException e, string strIn = "")
        {
            foreach (var vErr in e.EntityValidationErrors)
                foreach (var err in vErr.ValidationErrors)
                    ModelState.AddModelError("", err.ErrorMessage);
        }

        #endregion

        #region SessionAtributes

        protected Usuario Session_Usuario
        {
            get
            {
                if (Session["SESSION_USUARIO"] == null)
                    return null;
                else
                    return Session["SESSION_USUARIO"] as Usuario;
            }
            set
            {
                Session["SESSION_USUARIO"] = value;
            }
        }

        #endregion

    }
}