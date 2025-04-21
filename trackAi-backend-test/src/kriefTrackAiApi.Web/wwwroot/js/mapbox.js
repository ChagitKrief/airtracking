
function getCenterCoordinates(geoJson) {
  const defaultCenter = [35.2137, 31.7683];

  try {
    if (
      geoJson &&
      geoJson.data?.shipmentGeoJSON?.features?.length > 0
    ) {
      const lastFeature = geoJson.data.shipmentGeoJSON.features.at(-1);
      if (
        lastFeature?.properties?.longitude &&
        lastFeature?.properties?.latitude
      ) {
        return [
          lastFeature.properties.longitude,
          lastFeature.properties.latitude,
        ];
      }
    }
  } catch (e) {
    console.warn("Error getting center:", e);
  }

  return defaultCenter;
}

let centerCoordinates = [35.2137, 31.7683];

window.initMapbox = function (token, geoJsonString) {
  if (window.mapboxMapInitialized) return;
  window.mapboxMapInitialized = true;

  let rawData;
  try {
    rawData = JSON.parse(geoJsonString);
    centerCoordinates = getCenterCoordinates(rawData);
  } catch (err) {
    console.warn("Invalid GeoJSON, loading empty map.");
    rawData = null;
  }

  // Try to extract GeoJSON
  let geoJson = rawData?.data?.shipmentGeoJSON ?? rawData;

  // Fallback: If no GeoJSON or it's invalid, use an empty FeatureCollection
  const isValidGeoJson =
    geoJson &&
    geoJson.type === "FeatureCollection" &&
    Array.isArray(geoJson.features);

  if (!isValidGeoJson) {
    geoJson = {
      type: "FeatureCollection",
      features: [],
    };
    centerCoordinates = [35.2137, 31.7683]; // Or any default center like Tel Aviv
  }

  // Create the map
  mapboxgl.accessToken = token;
  const map = new mapboxgl.Map({
    container: "map",
    style: "mapbox://styles/mapbox/streets-v12",
    center: centerCoordinates,
    zoom: 6,
  });

  // Load minimal map if empty
  if (!isValidGeoJson || geoJson.features.length === 0) {
    return;
  }

  // Full map load with all layers and symbols
  map.loadImage("/icons/navigation.png", (error, image) => {
    if (!error && !map.hasImage("navigation")) {
      map.addImage("navigation", image);
    }
  });

  map.on("load", () => {
    map.addSource("route", { type: "geojson", data: geoJson });

    map.addLayer({
      id: "route",
      type: "line",
      source: "route",
      layout: { "line-join": "round", "line-cap": "round" },
      paint: { "line-color": "#0380fc", "line-width": 2 },
    });

    // Symbol layers
    const symbols = ["ferry", "harbor", "marker"];
    symbols.forEach((symbol) => {
      map.addLayer({
        id: `${symbol}-layer`,
        type: "symbol",
        source: "route",
        filter: ["==", ["get", "marker-symbol"], symbol],
        layout: {
          "icon-image": symbol,
          "icon-size": 1,
          "icon-allow-overlap": true,
        },
      });
    });

    // Load symbol images
    map.loadImage("/icons/nav.png", (e, img) => {
      if (!e && !map.hasImage("ferry")) map.addImage("ferry", img);
    });
    map.loadImage("/icons/marker.png", (e, img) => {
      if (!e && !map.hasImage("marker")) map.addImage("marker", img);
    });
    map.loadImage("/icons/harbor.png", (e, img) => {
      if (!e && !map.hasImage("harbor")) map.addImage("harbor", img);
    });

    // Dot-point (last known location)
    const lastFeature = geoJson.features.at(-1);
    if (lastFeature?.geometry?.coordinates) {
      map.addSource("dot-point", {
        type: "geojson",
        data: {
          type: "FeatureCollection",
          features: [
            {
              type: "Feature",
              properties: lastFeature.properties,
              geometry: {
                type: "Point",
                coordinates: lastFeature.geometry.coordinates,
              },
            },
          ],
        },
      });

      let isVisible = true;
      setInterval(() => {
        if (map.getLayer("dot-point")) {
          map.setPaintProperty(
            "dot-point",
            "icon-opacity",
            isVisible ? 1 : 0.2
          );
          isVisible = !isVisible;
        }
      }, 500);

      map.addLayer({
        id: "dot-point",
        type: "symbol",
        source: "dot-point",
        layout: {
          "icon-image": "navigation",
          "icon-allow-overlap": true,
          "symbol-z-order": "source",
          "icon-rotate": ["get", "course"],
        },
      });
    }
  });
};
