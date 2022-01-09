﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Models;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Models.Entities.Centres;
using BiblioMit.Models.Entities.Digest;
using BiblioMit.Models.Entities.Environmental;
using BiblioMit.Models.Entities.Environmental.Plancton;
using BiblioMit.Models.Entities.Ads;
using BiblioMit.Models.Entities.Variables;

namespace BiblioMit.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            if(builder == null) throw new ArgumentNullException(nameof(builder));
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();
                b.HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });
            builder.Entity<ApplicationRole>()
                .HasMany(e => e.Users)
                .WithOne()
                .HasForeignKey(e => e.RoleId)
                .IsRequired();

            builder.Entity<PlantProduct>(a => {
                a.HasKey(p => new { p.PlantId, p.ProductId });

                a.HasOne(md => md.Plant)
                    .WithMany(d => d.Products)
                    .HasForeignKey(md => md.PlantId);

                a.HasOne(md => md.Product)
                    .WithMany(d => d.Plants)
                    .HasForeignKey(md => md.ProductId);
            });
            builder.Entity<PlanktonAssayEmail>(a => {
                a.HasKey(p => new { p.PlanktonAssayId, p.EmailId });

                a.HasOne(md => md.PlanktonAssay)
                    .WithMany(d => d.Emails)
                    .HasForeignKey(md => md.PlanktonAssayId);

                a.HasOne(md => md.Email)
                    .WithMany(d => d.PlanktonAssayEmails)
                    .HasForeignKey(md => md.EmailId);
            });
            builder.Entity<Phytoplankton>(a => {
                a.HasKey(p => new { p.PlanktonAssayId, p.SpeciesId });

                a.HasOne(md => md.PlanktonAssay)
                    .WithMany(d => d.Phytoplanktons)
                    .HasForeignKey(md => md.PlanktonAssayId);

                a.HasOne(md => md.Species)
                    .WithMany(d => d.Phytoplanktons)
                    .HasForeignKey(md => md.SpeciesId);
            });
            builder.Entity<Analist>(a =>
            {
                a.HasIndex(p => p.NormalizedName).IsUnique();
                a.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<SamplingEntity>(s =>
            {
                s.HasIndex(p => p.NormalizedName).IsUnique();
                s.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<Email>(e =>
            {
                e.HasIndex(p => p.Address).IsUnique();
                e.Property(p => p.Address).IsRequired();
            });
            builder.Entity<Laboratory>(l =>
            {
                l.HasIndex(p => p.NormalizedName).IsUnique();
                l.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<Station>(s =>
            {
                s.HasIndex(p => p.NormalizedName).IsUnique();
                s.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<PhylogeneticGroup>(s =>
            {
                s.HasIndex(p => p.NormalizedName).IsUnique();
                s.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<GenusPhytoplankton>(s =>
            {
                s.HasIndex(p => p.NormalizedName).IsUnique();
                s.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<SpeciesPhytoplankton>(s =>
            {
                s.HasIndex(p => new { p.GenusId, p.NormalizedName }).IsUnique();
                s.Property(p => p.GenusId).IsRequired();
                s.Property(p => p.NormalizedName).IsRequired();
            });
            builder.Entity<Phone>(s =>
            {
                s.HasIndex(p => p.Number).IsUnique();
                s.Property(p => p.Number).IsRequired();
            });
            builder.Entity<AreaCodeProvince>(a => {
                a.HasKey(p => new { p.AreaCodeId, p.ProvinceId });

                a.HasOne(md => md.AreaCode)
                    .WithMany(d => d.AreaCodeProvinces)
                    .HasForeignKey(md => md.AreaCodeId);

                a.HasOne(md => md.Province)
                    .WithMany(d => d.AreaCodeProvinces)
                    .HasForeignKey(md => md.ProvinceId);
            });
            builder.Entity<Commune>()
                .HasOne(p => p.Province)
                .WithMany(p => p.Communes)
                .HasForeignKey(i => i.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Province>(r =>
            {
                r.HasOne(p => p.Region)
                .WithMany(p => p.Provinces)
                .HasForeignKey(i => i.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
                r.HasMany(p => p.Communes)
                .WithOne(c => c.Province)
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Region>()
                .HasMany(r => r.Provinces)
                .WithOne(p => p.Region)
                .HasForeignKey(r => r.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Polygon>(p =>
            {
                p.HasOne(p => p.Psmb)
                .WithOne(p => p.Polygon)
                .HasForeignKey<Psmb>(i => i.PolygonId)
                .OnDelete(DeleteBehavior.Restrict);

                p.HasOne(p => p.CatchmentArea)
                .WithOne(p => p.Polygon)
                .HasForeignKey<CatchmentArea>(i => i.PolygonId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Psmb>(a => {
                a.HasIndex(p => new { p.Code, p.CommuneId }).IsUnique();
                a.Property(p => p.Code).IsRequired();
                a.HasIndex(p => p.Acronym).IsUnique();
                a.HasDiscriminator<PsmbType>(nameof(Psmb.Discriminator))
                .HasValue<Craft>(PsmbType.Craft)
                .HasValue<Farm>(PsmbType.Farm)
                .HasValue<NaturalBed>(PsmbType.NaturalBed)
                .HasValue<Plant>(PsmbType.Plant)
                .HasValue<PsmbArea>(PsmbType.PsmbArea)
                .HasValue<ResearchCentre>(PsmbType.ResearchCentre);
            });
            builder.Entity<Locality>()
                .HasDiscriminator<LocalityType>(nameof(Locality.Discriminator))
                .HasValue<Region>(LocalityType.Region)
                .HasValue<Province>(LocalityType.Province)
                .HasValue<Commune>(LocalityType.Commune);
            builder.Entity<SernapescaDeclaration>(d =>
            {
                d.HasDiscriminator<DeclarationType>(nameof(SernapescaDeclaration.Discriminator))
                .HasValue<SeedDeclaration>(DeclarationType.Seed)
                .HasValue<HarvestDeclaration>(DeclarationType.Harvest)
                .HasValue<SupplyDeclaration>(DeclarationType.Supply)
                .HasValue<ProductionDeclaration>(DeclarationType.Production);
                d.HasIndex(p => new { p.Discriminator, p.DeclarationNumber }).IsUnique();
                d.Property(p => p.DeclarationNumber).IsRequired();
                d.Property(p => p.Discriminator).IsRequired();
            });
            builder.Entity<DeclarationDate>(d =>
            {
                d.HasIndex(p => new { p.SernapescaDeclarationId, p.Date, p.RawMaterial, p.ProductionType, p.ItemType }).IsUnique();
                d.Property(p => p.SernapescaDeclarationId).IsRequired();
                d.Property(p => p.Date).IsRequired();
            });
            builder.Entity<Variable>(d =>
            {
                d.HasOne(p => p.Psmb)
                .WithMany(p => p.Variables)
                .HasForeignKey(i => i.PsmbId)
                .OnDelete(DeleteBehavior.Restrict);
                d.HasOne(p => p.VariableType)
                .WithMany(p => p.Variables)
                .HasForeignKey(i => i.VariableTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Post>()
                .HasMany(p => p.Replies)
                .WithOne(p => p.Post)
                .HasForeignKey(p => p.PostId)
                .OnDelete(DeleteBehavior.NoAction);
        }
        public DbSet<VariableType> VariableTypes { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Btn> Btns { get; set; }
        public DbSet<Caption> Captions { get; set; }
        public DbSet<Img> Imgs { get; set; }
        public DbSet<Rgb> Rgbs { get; set; }
        public DbSet<DeclarationDate> DeclarationDates { get; set; }
        public DbSet<SupplyDeclaration> SupplyDeclarations { get; set; }
        public DbSet<HarvestDeclaration> HarvestDeclarations { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<ProductionDeclaration> ProductionDeclarations { get; set; }
        public DbSet<SeedDeclaration> SeedDeclarations { get; set; }
        public DbSet<IdentityUserClaim<string>> IdentityUserClaims { get; set; }
        public DbSet<IdentityUserRole<string>> IdentityUserRoles { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<Analist> Analists { get; set; }
        public DbSet<Analysis> Analyses { get; set; }
        public DbSet<AreaCode> AreaCodes { get; set; }
        public DbSet<AreaCodeProvince> AreaCodeProvinces { get; set; }
        public DbSet<Psmb> Psmbs { get; set; }
        public DbSet<PsmbArea> PsmbAreas { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<ResearchCentre> ResearchCentres { get; set; }
        public DbSet<NaturalBed> NaturalBeds { get; set; }
        public DbSet<Craft> Crafts { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<PlantProduct> PlantProducts { get; set; }
        public DbSet<Registry> Registries { get; set; }
        public DbSet<Header> Headers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Commune> Communes { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Coordinate> Coordinates { get; set; }
        public DbSet<Census> Census { get; set; }
        public DbSet<CatchmentArea> CatchmentAreas { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<PlanktonAssayEmail> PlanktonAssayEmails { get; set; }
        public DbSet<PlanktonAssay> PlanktonAssays { get; set; }
        public DbSet<SamplingEntity> SamplingEntities { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<InputFile> InputFiles { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<PhylogeneticGroup> PhylogeneticGroups { get; set; }
        public DbSet<GenusPhytoplankton> GenusPhytoplanktons { get; set; }
        public DbSet<IdentityUserClaim<string>> IdentityUserClaim { get; set; }
        public DbSet<IdentityUserRole<string>> IdentityUserRole { get; set; }
        public DbSet<PlanktonUser> PlanktonUsers { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }
        public DbSet<Larva> Larvas { get; set; }
        public DbSet<Larvae> Larvaes { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Origin> Origins { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Phytoplankton> Phytoplanktons { get; set; }
        public DbSet<SernapescaEntry> SernapescaEntries { get; set; }
        public DbSet<SernapescaDeclaration> SernapescaDeclarations { get; set; }
        //public DbSet<PlataformaUser> PlataformaUser { get; set; }
        //public DbSet<Platform> Platform { get; set; }
        public DbSet<Polygon> Polygons { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReply> PostReplies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<ReproductiveStage> ReproductiveStages { get; set; }
        public DbSet<Sampling> Samplings { get; set; }
        public DbSet<Seed> Seeds { get; set; }
        public DbSet<Soft> Softs { get; set; }
        public DbSet<Spawning> Spawnings { get; set; }
        public DbSet<Specie> Species { get; set; }
        public DbSet<SpecieSeed> SpecieSeeds { get; set; }
        public DbSet<SpeciesPhytoplankton> SpeciesPhytoplanktons { get; set; }
        public DbSet<Talla> Tallas { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Valve> Valves { get; set; }
        public override int SaveChanges() => base.SaveChanges();
    }
}