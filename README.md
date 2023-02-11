# Life360Wrapper
A C# wrapper for the Life360 API, and an interactive map to display trips using MapBox

I do intend on continuing this

Thanks to this person for most of the api endpoints [here](https://github.com/kaylathedev/life360-node-api)

```
// initialize the class
Life360Wrapper l360 = new Life360Wrapper();

// login
if (l360.Login("username", "password"))
{
    // do stuff here
}

// get circles
CIRCLES circles = l360.GetCircles();

// get a circle
CIRCLE_INFO circle_info = l360.GetCircle(circle_id);

// get trips for a user
TRIPS trips = l360.GetTrips(circle_id, user_id);

// get trip data from a specific trip
List<WAYPOINT> waypoints = l360.GetTripData(circle_id, user_id, trip_id);
// or
string json_trip_data = l360.GetRawTripData(circle_id, user_id, trip_id);

// creating a link for the interactive map
List<string> coords = new List<string>();
foreach(var wp in waypoints)
{
    coords.Add($"{wp.lat},{wp.lon}");
}
string s_coords = string.Join(";", coords);
string map_link = $"{location where the php is hosted}/map.php?coords={s_coords}";

// attempt to force a location update
string request_id = l360.GetLocationRequestId(circle_id, user_id);
if (request_id != "error")
    l360.RequestLocationUpdate(request_id);
```
