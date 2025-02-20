using GigFinder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GigFinder.Utils
{
    public static class UserUtils
    {
        public static User GetCurrentUser()
        {
            return HttpContext.Current?.Items["User"] as User;
        }
    }
}