using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using PolesTaxi.DriverService.Entity.Repository;
using PolesTaxi.DriverService.Entity.Service;
using PolesTaxi.Shared.Errors;
using PolesTaxi.Shared.Types;

namespace PolesTaxi.DriverService.Repository.Mongo;

public sealed class DriverMongoRepository
{
    private readonly IMongoCollection<DriverDocument> _drivers;

    public DriverMongoRepository(IMongoCollection<DriverDocument> drivers)
    {
        _drivers = drivers;
    }

    public async Task<Driver> Insert(Driver driver, CancellationToken cancellationToken)
    {
        var doc = ToDocument(driver);
        try
        {
            await _drivers.InsertOneAsync(doc, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Code == 11000)
        {
            throw AppException.Conflict("driver already exists for user");
        }
        return ToDomain(doc);
    }

    public async Task<Driver> GetByUserId(string userId, CancellationToken cancellationToken)
    {
        var doc = await _drivers.Find(x => x.UserId == userId).FirstOrDefaultAsync(cancellationToken);
        if (doc is null) throw AppException.NotFound("driver not found");
        return ToDomain(doc);
    }

    private static DriverDocument ToDocument(Driver driver) => new()
    {
        Id = string.IsNullOrEmpty(driver.Id) ? ObjectId.GenerateNewId().ToString() : driver.Id,
        UserId = driver.UserId,
        TaxiType = driver.TaxiType.ToWire(),
        LicensePlate = driver.LicensePlate,
        Status = StatusToWire(driver.Status),
        Location = ToGeo(driver.Location),
        CreatedAt = driver.CreatedAt,
        UpdatedAt = driver.UpdatedAt
    };

    private static Driver ToDomain(DriverDocument doc) => new()
    {
        Id = doc.Id,
        UserId = doc.UserId,
        TaxiType = EnumMaps.TaxiTypeFromWire(doc.TaxiType),
        LicensePlate = doc.LicensePlate,
        Status = StatusFromWire(doc.Status),
        Location = doc.Location is null ? null : new GeoPoint
        {
            Latitude = doc.Location.Coordinates.Latitude,
            Longitude = doc.Location.Coordinates.Longitude
        },
        CreatedAt = doc.CreatedAt,
        UpdatedAt = doc.UpdatedAt
    };

    private static GeoJsonPoint<GeoJson2DGeographicCoordinates>? ToGeo(GeoPoint? point)
    {
        if (point is null) return null;
        return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(point.Longitude, point.Latitude));
    }

    private static string StatusToWire(DriverStatus status) => status switch
    {
        DriverStatus.Offline => "offline",
        DriverStatus.Available => "available",
        DriverStatus.OnTrip => "on-trip",
        _ => "unspecified"
    };

    private static DriverStatus StatusFromWire(string value) => value switch
    {
        "offline" => DriverStatus.Offline,
        "available" => DriverStatus.Available,
        "on-trip" => DriverStatus.OnTrip,
        _ => DriverStatus.Unspecified
    };
}