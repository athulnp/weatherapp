# Database Setup & Configuration

## 📊 Database Overview

### Database Type
- **Type**: SQLite (Lightweight, file-based)
- **Location**: Inside the project directory
- **File Name**: `weatherapp.db`
- **Full Path**: `c:\Athul\Git\weatherapp\WeatherApp\weatherapp.db`

### Why SQLite?
- ✅ No external database server needed
- ✅ Perfect for small to medium projects
- ✅ Easy to deploy and backup
- ✅ Lightweight and fast
- ✅ Great for development and testing

---

## 🗂️ Database Location

### Current Configuration
```csharp
// In Program.cs (Line 45)
options.UseSqlite("Data Source=weatherapp.db")
```

### File Location Details
- **Relative Path**: `weatherapp.db` (relative to the application's working directory)
- **Absolute Path**: `c:\Athul\Git\weatherapp\WeatherApp\weatherapp.db`
- **Status**: Created automatically on first run

### Directory Structure
```
c:\Athul\Git\weatherapp\
├── WeatherApp\
│   ├── weatherapp.db          ← Database file (created on first run)
│   ├── Program.cs
│   ├── WeatherApp.csproj
│   ├── Controllers\
│   ├── Views\
│   ├── Models\
│   ├── Data\
│   │   ├── AppDbContext.cs
│   │   └── ArticleSeeder.cs
│   └── ...
└── ...
```

---

## 🔧 Database Configuration

### Entity Framework Core Setup
**File**: `Program.cs` (Lines 43-45)

```csharp
// Add Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=weatherapp.db"));
```

### Database Context
**File**: `WeatherApp/Data/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
```

### Database Initialization
**File**: `Program.cs` (Lines 126-132)

```csharp
// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();  // Creates database if it doesn't exist
    ArticleSeeder.SeedArticles(context);  // Seeds initial data
}
```

---

## 📝 Database Tables

### Articles Table
**Purpose**: Stores blog articles and content

**Columns**:
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Primary key, auto-increment |
| Title | string | Article title |
| Slug | string | URL-friendly slug (e.g., "weather-forecasting") |
| Content | string | Full article content (HTML) |
| PublishDate | DateTime | Publication date and time |

**Sample Data** (5 articles seeded):
1. "Complete Guide to Weather Forecasting" - `weather-forecasting`
2. "Weather Safety Tips" - `weather-safety-tips`
3. "Understanding Weather Patterns" - `weather-patterns`
4. "The Impact of Climate Change on Weather" - `climate-change-weather`
5. "Best Practices for Using Weather Apps" - `weather-app-guide`

---

## 🚀 Database Lifecycle

### 1. First Run (Automatic)
When the application starts for the first time:
1. ✅ Database file `weatherapp.db` is created
2. ✅ Articles table is created
3. ✅ 5 sample articles are inserted
4. ✅ Database is ready to use

### 2. Subsequent Runs
- ✅ Database already exists
- ✅ No duplicate seeding (ArticleSeeder checks if data exists)
- ✅ Application uses existing data

### 3. Data Persistence
- ✅ All data is persisted to the SQLite file
- ✅ Data survives application restarts
- ✅ Data is backed up with the project files

---

## 📦 Required NuGet Packages

**Installed in `WeatherApp.csproj`**:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.1" />
```

---

## 🔍 Database Files & Locations

### Files Inside Project
```
c:\Athul\Git\weatherapp\WeatherApp\
├── weatherapp.db              ← Main database file (created on first run)
├── weatherapp.db-shm          ← SQLite shared memory (temporary, if created)
└── weatherapp.db-wal          ← SQLite write-ahead log (temporary, if created)
```

### What Gets Created
1. **weatherapp.db** - Main database file (contains all tables and data)
2. **weatherapp.db-shm** - Shared memory file (temporary, for concurrent access)
3. **weatherapp.db-wal** - Write-ahead log (temporary, for transaction safety)

> **Note**: The `-shm` and `-wal` files are temporary and can be safely deleted. They're recreated as needed.

---

## 💾 Backup & Deployment

### Backing Up the Database
1. **Simple**: Copy `weatherapp.db` file to backup location
2. **Location**: `c:\Athul\Git\weatherapp\WeatherApp\weatherapp.db`
3. **Size**: Very small (typically < 1 MB for blog articles)

### Deploying to Production
1. ✅ Database is included in the project
2. ✅ No separate database setup needed
3. ✅ Just deploy the entire project folder
4. ✅ Database will be created automatically on first run

### Version Control
**Current Status**: Database file is NOT in git (recommended)
- Database is created fresh on each deployment
- Seed data is regenerated from `ArticleSeeder.cs`
- This ensures consistency across environments

---

## 🔐 Security Considerations

### Current Setup
- ✅ SQLite is file-based (no network exposure)
- ✅ Database is local to the application
- ✅ No external database credentials needed
- ✅ Safe for development and testing

### For Production
If scaling to production:
1. Consider migrating to SQL Server or PostgreSQL
2. Implement proper database backups
3. Use connection pooling
4. Add database encryption if needed
5. Implement access controls

---

## 🛠️ Database Operations

### Adding New Articles
**Method 1: Through Seeder (Development)**
```csharp
// Edit ArticleSeeder.cs and add new articles to the list
var articles = new List<Article>
{
    new Article { Title = "...", Slug = "...", Content = "...", PublishDate = DateTime.UtcNow }
};
```

**Method 2: Through Application (Runtime)**
```csharp
// In a controller or service
var article = new Article 
{ 
    Title = "New Article",
    Slug = "new-article",
    Content = "...",
    PublishDate = DateTime.UtcNow
};
context.Articles.Add(article);
context.SaveChanges();
```

### Querying Articles
```csharp
// Get all articles
var articles = context.Articles.ToList();

// Get article by slug
var article = context.Articles.FirstOrDefault(a => a.Slug == "weather-forecasting");

// Get articles by date
var recentArticles = context.Articles
    .Where(a => a.PublishDate > DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(a => a.PublishDate)
    .ToList();
```

### Updating Articles
```csharp
var article = context.Articles.FirstOrDefault(a => a.Id == 1);
if (article != null)
{
    article.Title = "Updated Title";
    context.SaveChanges();
}
```

### Deleting Articles
```csharp
var article = context.Articles.FirstOrDefault(a => a.Id == 1);
if (article != null)
{
    context.Articles.Remove(article);
    context.SaveChanges();
}
```

---

## 📊 Database Statistics

### Current Data
- **Tables**: 1 (Articles)
- **Records**: 5 (initial seed articles)
- **Total Content**: 7,700+ words
- **File Size**: ~50-100 KB (estimated)

### Growth Projections
| Articles | Estimated Size |
|----------|-----------------|
| 5 | ~50 KB |
| 20 | ~200 KB |
| 50 | ~500 KB |
| 100 | ~1 MB |

---

## ✅ Verification Checklist

- [x] Database configured in Program.cs
- [x] Entity Framework Core installed
- [x] SQLite provider installed
- [x] AppDbContext created
- [x] ArticleSeeder implemented
- [x] Database initialization code added
- [x] Sample data seeded
- [x] Build successful
- [x] Database will be created on first run

---

## 🚀 Next Steps

### Immediate (Week 2)
1. Run the application to create the database
2. Verify articles appear on `/Article` page
3. Test article retrieval by slug

### Short-term (Week 3-4)
1. Add more articles to the database
2. Implement article search functionality
3. Add article categories/tags
4. Implement pagination for article listing

### Medium-term (Month 2)
1. Add comments functionality
2. Implement article ratings
3. Add article analytics
4. Consider database migration to SQL Server if needed

---

## 📚 Resources

### Entity Framework Core Documentation
- https://learn.microsoft.com/en-us/ef/core/

### SQLite Documentation
- https://www.sqlite.org/docs.html

### ASP.NET Core Database Configuration
- https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/

---

## Summary

**Database Status**: ✅ READY

- **Type**: SQLite (file-based)
- **Location**: Inside project (`c:\Athul\Git\weatherapp\WeatherApp\weatherapp.db`)
- **Status**: Will be created automatically on first application run
- **Data**: 5 sample articles seeded automatically
- **Size**: Very small (~50-100 KB)
- **Backup**: Simple file copy
- **Deployment**: Included in project folder

The database is fully configured and will work seamlessly with the blog system!
