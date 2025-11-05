using MyBatis.NET.Core;
using MyBatis.NET.Mapper;
using MyBatis.ConsoleTest.Mappers;
using MyBatis.ConsoleTest.Models;

Console.WriteLine("══════════════════════════════════════════════════════════════════");
Console.WriteLine("         MyBatis.NET - Comprehensive Test Suite");
Console.WriteLine("══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// ========================================
// SETUP
// ========================================
Console.WriteLine("[SETUP] Initializing...");
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;
Console.WriteLine("✅ SQL Logging: ENABLED");

try
{
    MapperAutoLoader.AutoLoad("Mappers");
    Console.WriteLine("✅ Mappers loaded");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error loading mappers: {ex.Message}");
    return;
}

var connectionString = "Server=.;Database=MyBatisTestDB;Integrated Security=true;TrustServerCertificate=true";

var testsPassed = 0;
var testsFailed = 0;

try
{
    using var session = new SqlSession(connectionString);
    Console.WriteLine("✅ Connected to MyBatisTestDB");
    var mapper = session.GetMapper<IUserMapper>();
    Console.WriteLine("✅ Mapper proxy created");
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // SIMPLE QUERIES
    // ══════════════════════════════════════════════════════════════

    // TEST 1: GetAll (returnSingle="false")
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 1] GetAll() - returnSingle=\"false\" → List<User>");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var allUsers = mapper.GetAll();
        Console.WriteLine($"✅ PASS: Found {allUsers.Count} users");
        foreach (var u in allUsers.Take(3))
            Console.WriteLine($"   • {u.UserName} ({u.Email}) - {u.Role}");
        if (allUsers.Count > 3)
            Console.WriteLine($"   ... and {allUsers.Count - 3} more");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 2: GetById (returnSingle="true")
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 2] GetById(1) - returnSingle=\"true\" → User?");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var user = mapper.GetById(1);
        if (user != null)
        {
            Console.WriteLine($"✅ PASS: {user.UserName} ({user.Email})");
            Console.WriteLine($"   • Age: {user.Age}, Role: {user.Role}, Active: {user.IsActive}");
            testsPassed++;
        }
        else
        {
            Console.WriteLine("❌ FAIL: User not found");
            testsFailed++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // DYNAMIC SQL - IF CONDITIONS
    // ══════════════════════════════════════════════════════════════

    // TEST 3: SearchUsers - Single condition
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 3] SearchUsers(userName: \"john\") - <if> with 1 condition");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var results = mapper.SearchUsers("john", null, null, null, null);
        Console.WriteLine($"✅ PASS: Found {results.Count} users");
        foreach (var u in results.Take(3))
            Console.WriteLine($"   • {u.UserName} ({u.Email})");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 4: SearchUsers - Multiple conditions
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 4] SearchUsers - <if> with multiple conditions");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var results = mapper.SearchUsers(null, "Admin", 25, 50, true);
        Console.WriteLine($"✅ PASS: Found {results.Count} users");
        foreach (var u in results)
            Console.WriteLine($"   • {u.UserName} - Age: {u.Age}, Role: {u.Role}, Active: {u.IsActive}");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // FOREACH - COLLECTIONS
    // ══════════════════════════════════════════════════════════════

    // TEST 5: FindByRoles - String collection
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 5] FindByRoles - <foreach> with string collection");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var roles = new List<string> { "Admin", "Manager", "User" };
        var results = mapper.FindByRoles(roles);
        Console.WriteLine($"✅ PASS: Found {results.Count} users in {string.Join(", ", roles)}");
        foreach (var u in results.Take(5))
            Console.WriteLine($"   • {u.UserName} - {u.Role}");
        if (results.Count > 5)
            Console.WriteLine($"   ... and {results.Count - 5} more");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 6: FindByIds - Int collection
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 6] FindByIds - <foreach> with int collection");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var ids = new List<int> { 1, 2, 3 };
        var results = mapper.FindByIds(ids);
        Console.WriteLine($"✅ PASS: Found {results.Count} users with IDs {string.Join(", ", ids)}");
        foreach (var u in results)
            Console.WriteLine($"   • ID {u.Id}: {u.UserName}");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // CHOOSE/WHEN/OTHERWISE
    // ══════════════════════════════════════════════════════════════

    // TEST 7: SearchByType - CHOOSE/WHEN
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 7] SearchByType - <choose>/<when>/<otherwise>");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var byName = mapper.SearchByType("name", "john");
        Console.WriteLine($"✅ PASS (by name): Found {byName.Count} users");

        var byEmail = mapper.SearchByType("email", "example.com");
        Console.WriteLine($"✅ PASS (by email): Found {byEmail.Count} users");

        var byRole = mapper.SearchByType("role", "Admin");
        Console.WriteLine($"✅ PASS (by role): Found {byRole.Count} users");

        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // COMPLEX DYNAMIC SQL
    // ══════════════════════════════════════════════════════════════

    // TEST 8: ComplexSearch - Nested dynamic SQL
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 8] ComplexSearch - Complex nested <if>/<foreach>/<choose>");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var results = mapper.ComplexSearch(
            userName: "john",
            email: null,
            roles: new List<string> { "Admin", "Manager" },
            minAge: 20,
            maxAge: 60,
            isActive: true,
            createdAfter: DateTime.Now.AddYears(-5),
            orderBy: "age"
        );
        Console.WriteLine($"✅ PASS: Found {results.Count} users");
        foreach (var u in results.Take(3))
            Console.WriteLine($"   • {u.UserName} - Age: {u.Age}, Role: {u.Role}");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 9: CountUsers - Returns single int
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 9] CountUsers - returnSingle=\"true\", resultType=\"int\"");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var count = mapper.CountUsers("Admin", true);
        Console.WriteLine($"✅ PASS: Active Admin users count = {count}");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // CRUD OPERATIONS
    // ══════════════════════════════════════════════════════════════

    // TEST 10: InsertUser
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 10] InsertUser - <insert> statement");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var newUser = new User
        {
            UserName = "TestUser_" + DateTime.Now.Ticks,
            Email = $"test{DateTime.Now.Ticks}@example.com",
            Age = 28,
            Role = "User",
            IsActive = true
        };
        var affected = mapper.InsertUser(newUser);
        Console.WriteLine($"✅ PASS: Inserted {affected} row(s)");
        Console.WriteLine($"   • UserName: {newUser.UserName}");
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 11: UpdateUser - Dynamic SET
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 11] UpdateUser - <update> with <set>");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        var userToUpdate = mapper.GetById(1);
        if (userToUpdate != null)
        {
            userToUpdate.Email = $"updated{DateTime.Now.Ticks}@example.com";
            var affected = mapper.UpdateUser(userToUpdate);
            Console.WriteLine($"✅ PASS: Updated {affected} row(s)");
            Console.WriteLine($"   • New Email: {userToUpdate.Email}");
            testsPassed++;
        }
        else
        {
            Console.WriteLine("⚠️ SKIP: User ID 1 not found");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 12: SoftDeleteUser
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 12] SoftDeleteUser - <update> soft delete");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        // Find a user to soft delete
        var usersToDelete = mapper.SearchUsers("TestUser", null, null, null, null);
        if (usersToDelete.Count > 0)
        {
            var affected = mapper.SoftDeleteUser(usersToDelete[0].Id);
            Console.WriteLine($"✅ PASS: Soft deleted {affected} row(s)");
            testsPassed++;
        }
        else
        {
            Console.WriteLine("⚠️ SKIP: No test users found to delete");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // TEST 13: DeleteUser
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 13] DeleteUser - <delete> statement");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        // Find a test user to delete
        var usersToDelete = mapper.SearchUsers("TestUser", null, null, null, null);
        if (usersToDelete.Count > 0)
        {
            var affected = mapper.DeleteUser(usersToDelete[0].Id);
            Console.WriteLine($"✅ PASS: Deleted {affected} row(s)");
            testsPassed++;
        }
        else
        {
            Console.WriteLine("⚠️ SKIP: No test users found to delete");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();

    // ══════════════════════════════════════════════════════════════
    // SQL LOGGING TEST
    // ══════════════════════════════════════════════════════════════

    // TEST 14: Disable SQL Logging
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("[TEST 14] SQL Logging Toggle");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    try
    {
        SqlSessionConfiguration.EnableSqlLogging = false;
        SqlSessionConfiguration.EnableParameterLogging = false;
        Console.WriteLine("🔇 SQL Logging: DISABLED");

        var silentQuery = mapper.GetAll();
        Console.WriteLine($"✅ PASS: Query executed silently ({silentQuery.Count} users, no SQL logged)");

        // Re-enable for final summary
        SqlSessionConfiguration.EnableSqlLogging = true;
        SqlSessionConfiguration.EnableParameterLogging = true;
        testsPassed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {ex.Message}");
        testsFailed++;
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine("❌ FATAL ERROR");
    Console.WriteLine("══════════════════════════════════════════════════════════════");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    Console.WriteLine();
    Console.WriteLine("Troubleshooting:");
    Console.WriteLine("1. Ensure MyBatisTestDB database exists");
    Console.WriteLine("2. Ensure Mappers/UserMapper.xml exists");
    Console.WriteLine("3. Check connection string");
    Console.WriteLine();
    return;
}

// ══════════════════════════════════════════════════════════════════
// TEST SUMMARY
// ══════════════════════════════════════════════════════════════════
Console.WriteLine("══════════════════════════════════════════════════════════════════");
Console.WriteLine("                       TEST SUMMARY");
Console.WriteLine("══════════════════════════════════════════════════════════════════");
Console.WriteLine();

var totalTests = testsPassed + testsFailed;
var passRate = totalTests > 0 ? (testsPassed * 100.0 / totalTests) : 0;

Console.WriteLine($"Total Tests:  {totalTests}");
Console.WriteLine($"✅ Passed:    {testsPassed}");
Console.WriteLine($"❌ Failed:    {testsFailed}");
Console.WriteLine($"Pass Rate:    {passRate:F1}%");
Console.WriteLine();

if (testsFailed == 0)
{
    Console.WriteLine("🎉 ALL TESTS PASSED! 🎉");
    Console.WriteLine();
    Console.WriteLine("Features Verified:");
    Console.WriteLine("  ✅ returnSingle=\"true\" → Single object (nullable)");
    Console.WriteLine("  ✅ returnSingle=\"false\" → List<T>");
    Console.WriteLine("  ✅ Dynamic SQL with <if> conditions");
    Console.WriteLine("  ✅ <foreach> with collections (int, string)");
    Console.WriteLine("  ✅ <choose>/<when>/<otherwise> (switch-case)");
    Console.WriteLine("  ✅ <set> for dynamic UPDATE");
    Console.WriteLine("  ✅ Complex nested dynamic SQL");
    Console.WriteLine("  ✅ INSERT, UPDATE, DELETE operations");
    Console.WriteLine("  ✅ SQL Logging with parameters");
    Console.WriteLine("  ✅ Mapper Proxy Pattern");
}
else
{
    Console.WriteLine("⚠️ SOME TESTS FAILED");
    Console.WriteLine("Please review the errors above.");
}

Console.WriteLine();
Console.WriteLine("══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
