using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

namespace FixtureApi.Models {
    public class BaseObject {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}