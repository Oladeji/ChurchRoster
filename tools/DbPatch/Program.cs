using Npgsql;
var cs = "Host=aws-1-ca-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.edxjeuoutitcdfuqzyxp;Password=Deji1@Akoms!;SSL Mode=Require;Trust Server Certificate=true";
await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();
await using var cmd = new NpgsqlCommand(
    "UPDATE roster_proposals SET status=@newStatus WHERE proposal_id=@id AND status=@oldStatus RETURNING proposal_id",
    conn);
cmd.Parameters.AddWithValue("@newStatus", "Archived");
cmd.Parameters.AddWithValue("@id", 11);
cmd.Parameters.AddWithValue("@oldStatus", "Processing");
var result = await cmd.ExecuteScalarAsync();
Console.WriteLine(result is not null ? $"Updated proposal {result} to Archived" : "No rows updated");
