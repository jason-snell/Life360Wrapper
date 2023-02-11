using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life360Wrapper
{
    class Life360Wrapper
    {
        private string AUTH_SECRET_TOKEN = "";
        public string access_token = "";

        private string api_endpoint = "https://api.life360.com";

        /*
         * gets a token from the main website that's used when doing the oauth
         */
        private bool GetAuthSecretToken()
        {
            var client = new RestClient("https://app.life360.com/config.js");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("AUTH_SECRET_TOKEN"))
                return false;

            string raw_json = response.Content.Replace("window.life360Config=Object.freeze(", "").Replace("\"}})", "\"}}");
            dynamic json = JsonConvert.DeserializeObject(raw_json);
            AUTH_SECRET_TOKEN = json.env.AUTH_SECRET_TOKEN;
            
            return true;
        }

        public Life360Wrapper()
        {
            GetAuthSecretToken();
        }

        /*
         * oauth login, returns false if there's an issue logging in
         */
        public bool Login(string username, string password)
        {
            //return true;

            var client = new RestClient($"{api_endpoint}/v3/oauth2/token.json");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", $"Basic {AUTH_SECRET_TOKEN}");
            request.AddHeader("content-type", "application/json");

            var body = new
            {
                username = username,
                password = password,
                grant_type = "password"
            };

            request.AddParameter(" application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("access_token"))
                return false;

            dynamic json = JsonConvert.DeserializeObject(response.Content);
            access_token = json.access_token;

            return true;
        }

        /*
         * returns all circles
         */
        public CIRCLES GetCircles()
        {
            CIRCLES circles = new CIRCLES();

            var client = new RestClient($"{api_endpoint}/v3/circles/");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {access_token}");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("circles"))
                return circles;

            circles = JsonConvert.DeserializeObject<CIRCLES>(response.Content);
            
            return circles;
        }

        /*
         * returns information about the requested circle
         * 
         * circle_id is returned from the GetCircles function
         */
        public CIRCLE_INFO GetCircle(string circle_id)
        {
            CIRCLE_INFO circle_info = new CIRCLE_INFO();

            var client = new RestClient($"{api_endpoint}/v3/circles/{circle_id}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {access_token}");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("members"))
                return circle_info;

            circle_info = JsonConvert.DeserializeObject<CIRCLE_INFO>(response.Content);

            return circle_info;
        }

        /*
         * returns some of the most recent trips for a user
         * 
         * circle_id is returned from the GetCircles function
         * user_id is returned from the GetCircle function
         */
        public TRIPS GetTrips(string circle_id, string user_id)
        {
            TRIPS trips = new TRIPS();

            var client = new RestClient($"{api_endpoint}/v3/circles/{circle_id}/users/{user_id}/driverbehavior/trips.json");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {access_token}");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("trips"))
                return trips;

            trips = JsonConvert.DeserializeObject<TRIPS>(response.Content);

            return trips;
        }

        /*
         * returns a list of waypoints from the trip, the api call returns a lot more and I attempted to make a class to properly return everything,
         * but it didn't work the first try so I just skipped it and returned what I needed from it
         * 
         * circle_id is returned from the GetCircles function
         * user_id is returned from the GetCircle function
         * trip_id is returned from the GetTrips function
         */
        public List<WAYPOINT> GetTripData(string circle_id, string user_id, string trip_id)
        {
            List<WAYPOINT> waypoints = new List<WAYPOINT>();

            var client = new RestClient($"{api_endpoint}/v3/circles/{circle_id}/users/{user_id}/driverbehavior/trips/{trip_id}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {access_token}");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("waypoints"))
                return waypoints;

            dynamic json = JsonConvert.DeserializeObject(response.Content);
            foreach(var waypoint in json.trip.waypoints)
            {
                var wp = new WAYPOINT();
                wp.lat = waypoint.lat;
                wp.lon = waypoint.lon;
                wp.speed = waypoint.speed;
                wp.timestamp = waypoint.timestamp;
                waypoints.Add(wp);
            }

            return waypoints;
        }

        /*
         * this is an extension to the function above, still didn't feel like properly returning the data in a class,
         * so this returns the json as a string
         * 
         * same parameters
        */
        public string GetRawTripData(string circle_id, string user_id, string trip_id)
        {
            var client = new RestClient($"{api_endpoint}/v3/circles/{circle_id}/users/{user_id}/driverbehavior/trips/{trip_id}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {access_token}");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("waypoints"))
                return "error";

            return response.Content;
        }

        /*
         * this api call is supposed to force a location update, doesn't always seem to work in my experience,
         * but it returns a 'requestId' which is passed into the function below
         */
        public string GetLocationRequestId(string circle_id, string user_id)
        {
            var client = new RestClient($"{api_endpoint}/v3/circles/{circle_id}/members/{user_id}/request.json");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", $"Bearer {access_token}");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("type", "location");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("isPollable"))
                return "error";

            dynamic json = JsonConvert.DeserializeObject(response.Content);
            string requestId = json.requestId;
            return requestId;
        }

        /*
         * this function returns updated location info when the status is 'A', it appears that you can continuously use the same,
         * requestId for new location updates, however, this ONLY returns location info. so calling the function above, followed by
         * GetCircle will return all updated information, assuming the location update request is complete
         */
        public string RequestLocationUpdate(string request_id)
        {
            var client = new RestClient($"{api_endpoint}/v3/circles/members/request/{request_id}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {access_token}");
            IRestResponse response = client.Execute(request);

            if ((int)response.StatusCode != 200 || !response.Content.Contains("requestId"))
                return "error";

            dynamic json = JsonConvert.DeserializeObject(response.Content);
            string status = json.status;

            if (status != "A")
                return "not_ready";

            return response.Content;
        }
    }

    public class CIRCLE
    {
        public string id = "";
        public string name = "";
        public string color = "";
        public string type = "";
        public string createdAt = "";
        public string memberCount = "";
        public string unreadMessages = "";
        public string unreadNotifications = "";

        public class features
        {
            public string ownerId = "";
            public string premium = "";
            public int locationUpdatesLeft = 0;
            public string priceMonth = "";
            public string priceYear = "";
            public string skuId = "";
            public string skuTier = "";
        }
    }

    public class COMMUNICATION
    {
        public string channel = "";
        public string value = "";
        public string type = "";
    }

    public class MEMBER
    {
        public class _features
        {
            public string device = "";
            public string smartphone = "";
            public string nonSmartphoneLocating = "";
            public string geofencing = "";
            public string shareLocation = "";
            public string shareOffTimestamp = "";
            public string disconnected = "";
            public string pendingInvite = "";
            public string mapDisplay = "";
        }

        public _features features = new _features();

        public class _issues
        {
            public string disconnected = "";
            public string type = "";
            public string status = "";
            public string title = "";
            public string dialog = "";
            public string action = "";
            public string toubleshooting = "";
        }

        public _issues issues = new _issues();

        public class _location
        {
            public string latitude = "";
            public string longitude = "";
            public string accuracy = "";
            public int startTimestamp = 0;
            public string endTimestamp = "";
            public int since = 0;
            public string timestamp = "";
            public string name = "";
            public string placeType = "";
            public string source = "";
            public string sourceId = "";
            public string address1 = "";
            public string address2 = "";
            public string shortAddress = "";
            public string inTransit = "";
            public string tripId = "";
            public string driveSDKStatus = "";
            public string battery = "";
            public string charge = "";
            public string wifiState = "";
            public double speed = 0;
            public string isDriving = "";
            public string userActivity = "";
        }

        public _location location = new _location();

        public List<COMMUNICATION> communications = new List<COMMUNICATION>();
        public string medical = "";
        public string relation = "";
        public string createdAt = "";
        public string activity = "";
        public string id = "";
        public string firstName = "";
        public string lastName = "";
        public string isAdmin = "";
        public string avatar = "";
        public string pinNumber = "";
        public string loginEmail = "";
        public string loginPhone = "";
    }

    public class CIRCLE_INFO
    {
        public CIRCLE circle = new CIRCLE();
        public List<MEMBER> members = new List<MEMBER>();
    }

    public class CIRCLES
    {
        public List<CIRCLE> circles = new List<CIRCLE>();
    }

    public class TRIP
    {
        public string userId = "";
        public string tripId = "";
        public int startTime = 0;
        public int endTime = 0;
        public double topSpeed = 0;
        public double averageSpeed = 0;
        public double distance = 0;
        public double duration = 0;
        public int speedingCount = 0;
        public int hardBrakingCount = 0;
        public int rapidAccelerationCount = 0;
        public int distractedCount = 0;
        public int crashCount = 0;
        public int score = 0;
        public int driveType = 0;
        public int userMode = 0;
        public int userTag = 0;
    }

    public class TRIPS
    {
        public List<TRIP> trips = new List<TRIP>();
    }

    public class WAYPOINT
    {
        public double lat = 0;
        public double speed = 0;
        public double accuracy = 0;
        public int timestamp = 0;
        public double lon = 0;
    } 
}
