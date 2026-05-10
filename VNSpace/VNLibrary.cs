using System;
using System.Collections.Generic;
using System.Text;

namespace VNSpace
{
    internal class VNLibrary
    {

        private List<UserEntry> vnList; //list of vns in library
        private List<string> categories;

        public VNLibrary()
        {
           this.vnList = new List<UserEntry>();
        }
    }
}
