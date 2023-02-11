<?php
$posted_coords = $_GET['coords'];
$coords = explode(';', $posted_coords);

$coords_array = array();
for ($i = 0; $i < count($coords); $i++)
{
    $tokens = explode(',', $coords[$i]);
    $lon = $tokens[0];
    $lat = $tokens[1];
    $comp = $lat . ',' . $lon;
    array_push($coords_array, '[' . $comp . ']');
}
?>

<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Map</title>
<meta name="viewport" content="initial-scale=1,maximum-scale=1,user-scalable=no">
<link href="https://api.mapbox.com/mapbox-gl-js/v2.12.0/mapbox-gl.css" rel="stylesheet">
<script src="https://api.mapbox.com/mapbox-gl-js/v2.12.0/mapbox-gl.js"></script>
<style>
body { margin: 0; padding: 0; }
#map { position: absolute; top: 0; bottom: 0; width: 100%; }
</style>
</head>
<body>
<div id="map"></div>
<script>

const coords = [
	<?php echo join(',', $coords_array); ?>
];

mapboxgl.accessToken = "";
const map = new mapboxgl.Map({
	container: "map",
	// Choose from Mapbox"s core styles, or make your own style with Mapbox Studio
	style: "mapbox://styles/mapbox/satellite-streets-v12",
	center: coords[Math.ceil(coords.length / 2)],
	zoom: 14
});	
 
map.on("load", () => {
	map.addSource("route", {
		"type": "geojson",
		"data": {
			"type": "Feature",
			"properties": {},
			"geometry": {
				"type": "LineString",
				"coordinates": coords
			}
		}
	});
	map.addLayer({
		"id": "route",
		"type": "line",
		"source": "route",
		"layout": {
			"line-join": "round",
			"line-cap": "round"
		},
		"paint": {
			"line-color": "#FF0000",
			"line-width": 2
		}
	});
	
	const start_marker = new mapboxgl.Marker()
		.setLngLat(coords[0])
		.addTo(map);
	
	// markers for each individual checkpoint, gets messy
	//for (let i = 1; i < coords.length - 1; i++)
	//{
	//	const marker2 = new mapboxgl.Marker({ color: "red" })
	//	.setLngLat(coords[i])
	//	.addTo(map);
	//}
	
	const end_marker = new mapboxgl.Marker({color: "green" })
		.setLngLat(coords[coords.length - 1])
		.addTo(map);
});
</script>
 
</body>
</html>