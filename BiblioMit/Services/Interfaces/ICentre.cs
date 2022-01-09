﻿using BiblioMit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public interface ICentre
    {
        IEnumerable<Psmb> GetFilteredCentres(int page, int rpp, string searchQuery);
    }
}
