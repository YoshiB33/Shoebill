﻿namespace Shoebill.Models.Api.Schemas;

public record Server_Database(
    string Id,
    Server_Database_Host Host,
    string Name,
    string Username,
    string Connections_from,
    int Max_connection,
    DB_Relationships Relationships
);

public record Server_Database_Host(string Adress, int Port);

public record DB_Relationships(Password Password);

public record Password(string Object, DB_Attributes Attributes);

public record DB_Attributes(string Password);