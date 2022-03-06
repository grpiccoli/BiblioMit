﻿using BiblioMit.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Security.Principal;

namespace BiblioMit.Views.Components.Nav
{
    public class NavViewComponent : ViewComponent
    {
        private readonly IStringLocalizer<NavViewComponent> _localizer;
        public NavViewComponent(
            IStringLocalizer<NavViewComponent> localizer
            )
        {
            _localizer = localizer;
        }
        public IViewComponentResult Invoke()
        {
            NavDDwnVM links = new ()
            {
                Logo = "fas fa-link",
                Title = _localizer["Links"],
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = _localizer["Main"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Index",
                                Name = _localizer["Home"],
                                Icon = "fas fa-home"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "About",
                                Name = _localizer["About us"],
                                Icon = "fas fa-question"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "Contact",
                                Name = _localizer["Contact"],
                                Icon = "fas fa-address-book"
                            }
                        }
                    },
                    new Section
                    {
                        Name = _localizer["Information"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Flowpaper",
                                Name = _localizer["User Manual"],
                                Icon = "far fa-question-circle"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "Privacy",
                                Name = _localizer["Privacy Policy"],
                                Icon = "fas fa-shield-alt"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "Terms",
                                Name = _localizer["Terms & Conditions"],
                                Icon = "fas fa-balance-scale"
                            }
                        }
                    }
                }
            };
            Collection<Link> producersLinks = new()
            {
                new Link
                {
                    Controller = "Ambiental",
                    Action = "Graph",
                    Name = _localizer["PSMBs"],
                    Icon = "fas fa-water"
                }
            };
            IIdentity? identity = User.Identity;
            if (identity == null)
            {
                throw new UnauthorizedAccessException();
            }

            HashSet<UserClaims> claims = ((ClaimsIdentity)identity).Claims
                .Where(c => Enum.IsDefined(typeof(UserClaims), c.Value))
                .Select(c => (UserClaims)Enum.Parse(typeof(UserClaims), c.Value))
                .ToHashSet();

            bool authenticated = identity.IsAuthenticated;
            bool admin = authenticated && User.IsInRole(RoleData.Administrator.ToString());
            bool editor = admin || (authenticated && User.IsInRole(RoleData.Editor.ToString()));
            bool webmaster = admin && claims.Contains(UserClaims.Webmaster);
            bool client = authenticated && User.IsInRole(RoleData.Client.ToString());
            bool centres = authenticated && claims.Contains(UserClaims.Centres);
            if (centres)
            {
                producersLinks.Add(new Link
                {
                    Controller = "Centres",
                    Action = "Producers",
                    Name = _localizer["Aquaculture Farms"],
                    Icon = "fas fa-industry"
                });
            }

            producersLinks.Add(new Link
            {
                Controller = "Centres",
                Action = "Research",
                Name = _localizer["Research Centres"],
                Icon = "fas fa-microscope"
            });
            NavDDwnVM producers = new ()
            {
                Logo = "fas fa-map-marker-alt",
                Title = _localizer["Maps"],
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = _localizer["Maps"],
                        Links = producersLinks
                    }
                }
            };
            NavDDwnVM boletin = new ()
            {
                Logo = "fas fa-chart-line",
                Title = _localizer["Reports"],
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = _localizer["Publications"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Boletin",
                                Action = "Index",
                                Name = _localizer["Production / Environmental"],
                                Icon = "fas fa-newspaper"
                            }
                        }
                    },
                    new Section
                    {
                        Name = _localizer["Website Analytics"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Analytics",
                                Name = _localizer["Web Analytics"],
                                Icon = "fas fa-poll"
                            }
                        }
                    },
                    new Section
                    {
                        Name = _localizer["Sistemas"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Simac",
                                Name = "SIMAC",
                                Icon = "fab fa-centos"
                            }
                        }
                    }
                }
            };
            NavDDwnVM publications = new ()
            {
                Logo = "fas fa-search",
                Title = _localizer["Search"],
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = _localizer["Engines"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Publications",
                                Action = "Index",
                                Name = _localizer["Library"],
                                Icon = "fas fa-book-reader"
                            },
                            new Link
                            {
                                Controller = "Contacts",
                                Action = "Index",
                                Name = _localizer["Contacts"],
                                Icon = "far fa-address-book"
                            },
                            new Link
                            {
                                Controller = "Publications",
                                Action = "Agenda",
                                Name = _localizer["Funding"],
                                Icon = "fas fa-hand-holding-usd"
                            },
                            //, "modal", "#modal-action"
                            new Link
                            {
                                Controller = "Home",
                                Action = "Search",
                                Name = _localizer["Website search"],
                                Icon = "fas fa-search"
                            }
                        }
                    }
                }
            };
            NavDDwnVM gallery = new ()
            {
                Logo = "fas fa-images",
                Title = _localizer["Images"],
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = _localizer["Catalogue"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Flowpaper",
                                Parameters = new Dictionary<string, string>{
                                    { "n", "gallery" }
                                },
                                Name = _localizer["Histopathology Gallery"],
                                Icon = "fas fa-disease"
                            }
                        }
                    }
                }
            };
            NavDDwnVM forum = new ()
            {
                Logo = "far fa-comment-dots",
                Title = _localizer["Networking"],
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = _localizer["Forums"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "UserManage",
                                Name = _localizer["Profile"],
                                Icon = "fas fa-user-edit"
                            },
                            new Link()
                            {
                                Controller = "Home",
                                Action = "Forum",
                                Name = _localizer["Forums"],
                                Icon = "fas fa-comments"
                            }
                        }
                    },
                    new Section
                    {
                        Name = _localizer["Survey"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Survey",
                                Name = _localizer["Tell us what you think"],
                                Icon = "fas fa-user-edit"
                            },
                            new Link()
                            {
                                Controller = "Home",
                                Action = "Responses",
                                Name = _localizer["Responses"],
                                Icon = "fas fa-comments"
                            }
                        }
                    }
                }
            };

            if (admin && claims.Contains(UserClaims.Digest))
            {
                boletin.Sections.Add(new Section
                {
                    Name = _localizer["Administration"],
                    Links = new Collection<Link>
                    {
                        new Link
                        {
                            Controller = "Entries",
                            Action = "Create",
                            Name = _localizer["Production"],
                            Icon = "fas fa-unlock-alt"
                        },
                        new Link
                        {
                            Controller = "Columnas",
                            Action = "Index",
                            Name = _localizer["Input Format"],
                            Icon = "fas fa-unlock-alt"
                        },
                        new Link
                        {
                            Controller = "Entries",
                            Action = "Index",
                            Name = _localizer["Uploaded Files"],
                            Icon = "fas fa-unlock-alt"
                        }
                    }
                });
            }

            if (admin && claims.Contains(UserClaims.PSMB))
            {
                boletin.Sections.Add(new Section
                {
                    Name = _localizer["Administration"],
                    Links = new Collection<Link>
                    {
                        new Link
                        {
                            Controller = "Entries",
                            Action = "CreateFito",
                            Name = _localizer["Environmental"],
                            Icon = "fas fa-globe-americas"
                        },
                        new Link
                        {
                            Controller = "Ambiental",
                            Action = "Graph",
                            Name = _localizer["PSMB statistics"],
                            Icon = "fas fa-chart-line"
                        },
                        new Link
                        {
                            Controller = "Ambiental",
                            Action = "PullPlankton",
                            Name = _localizer["Upload Errors"],
                            Icon = "fas fa-exclamation-triangle"
                        }
                    }
                });
                producers.Sections.Add(new Section
                {
                    Name = _localizer["Configuration"],
                    Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "VariableTypes",
                                Action = "Index",
                                Name = _localizer["Variable Types"],
                                Icon = "fas fa-water"
                            },
                            new Link
                            {
                                Controller = "Variables",
                                Action = "Index",
                                Name = _localizer["Custom Variables"],
                                Icon = "fas fa-industry"
                            }
                        }
                });
            }

            if (admin && claims.Contains(UserClaims.Forums))
            {
                forum.Sections.Add(new Section
                {
                    Name = _localizer["Administration"],
                    Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Fora",
                                Action = "Create",
                                Name = _localizer["Create Forum"],
                                Icon = "fas fa-comment-dots"
                            }
                        }
                });
            }

            if (admin && claims.Contains(UserClaims.Mitilidb))
            {
                gallery.Sections.Add(new Section
                {
                    Name = _localizer["Administration"],
                    Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Samplings",
                                Action = "Index",
                                Name = _localizer["Samplings"],
                                Icon = "fas fa-microscope"
                            },
                            new Link
                            {
                                Controller = "Individuals",
                                Action = "Index",
                                Name = _localizer["Subjects"],
                                Icon = "fas fa-info"
                            },
                            new Link
                            {
                                Controller = "Softs",
                                Action = "Index",
                                Name = _localizer["Softs"],
                                Icon = "fas fa-vial"
                            }
                        }
                });

                gallery.Sections.Add(new Section
                {
                    Name = _localizer["Images"],
                    Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Photos",
                                Action = "Index",
                                Name = _localizer["Image gallery"],
                                Icon = "fas fa-images"
                            }
                        }
                });
            }

            List<NavDDwnVM> model = new ()
            {
                links, producers, boletin, publications, gallery, forum
            };

            if (claims.Contains(UserClaims.Banners))
            {
                NavDDwnVM services = new ()
                {
                    Logo = "fas fa-concierge-bell",
                    Title = _localizer["Services"]
                };
                if (client || webmaster)
                {
                    services.Sections.Add(new Section
                    {
                        Name = _localizer["Clients"],
                        Links = new Collection<Link>
                            {
                                new Link
                                {
                                    Controller = "Payment",
                                    Action = "Index",
                                    Name = _localizer["Pay"],
                                    Icon = "fas fa-shopping-cart"
                                }
                            }
                    });
                }
                if (admin)
                {
                    services.Sections.Add(new Section
                    {
                        Name = _localizer["Advertisement"],
                        Links = new Collection<Link>
                            {
                                new Link
                                {
                                    Controller = "Banners",
                                    Action = "Index",
                                    Name = _localizer["Banners"],
                                    Icon = "fas fa-ad"
                                }
                            }
                    });
                }
                model.Add(services);
            }

            if (webmaster)
            {
                model.Add(new NavDDwnVM
                {
                    Logo = "fas fa-tools",
                    Title = _localizer["Administration"],
                    Sections = new Collection<Section>
                    {
                        new Section
                        {
                            Name = _localizer["Databases"],
                            Links = new Collection<Link>
                                {
                                    new Link
                                    {
                                        Controller = "Companies",
                                        Action = "Index",
                                        Name = _localizer["Companies and Institutions"]
                                    },
                                    new Link
                                    {
                                        Controller = "Centres",
                                        Action = "Index",
                                        Name = _localizer["Centres"]
                                    },
                                    new Link
                                    {
                                        Controller = "Coordinates",
                                        Action = "Index",
                                        Name = _localizer["Coordinates"]
                                    },
                                    new Link
                                    {
                                        Controller = "Productions",
                                        Action = "Index",
                                        Name = _localizer["Productions"]
                                    },
                                    new Link
                                    {
                                        Controller = "Contacts",
                                        Action = "Index",
                                        Name = _localizer["Contacts"]
                                    }
                                }
                        },
                        new Section
                        {
                            Name = _localizer["Users"],
                            Links = new Collection<Link>
                                {
                                    new Link
                                    {
                                        Controller = "User",
                                        Action = "Index",
                                        Name = _localizer["Users and claims"]
                                    },
                                    new Link
                                    {
                                        Controller = "AppRole",
                                        Action = "Index",
                                        Name = _localizer["Roles"]
                                    }
                                }
                        }
                    }
                });
            }
            DefaultModel result = new (new ReadOnlyCollection<NavDDwnVM>(model));
            return View(result);
        }
    }
    public class DefaultModel
    {
        public DefaultModel(ReadOnlyCollection<NavDDwnVM> navs)
        {
            Navs = navs;
        }
        public ReadOnlyCollection<NavDDwnVM> Navs { get; private set; }
        public LoginDropdownModel LoginModel { get; private set; } = new();
    }
    public class LoginDropdownModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
    public class NavDDwnVM
    {
        public string? Logo { get; set; }
        public string? Title { get; set; }
        public Collection<Section> Sections { get; internal set; } = new();
    }
    public class Section
    {
        public string? Name { get; set; }
        public Collection<Link> Links { get; internal set; } = new();
    }
    public class Link
    {
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public Dictionary<string, string> Parameters { get; internal set; } = new();
        public string? Icon { get; set; }
        public string? Name { get; set; }
    }
}
