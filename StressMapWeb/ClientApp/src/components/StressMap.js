import React, { useState, useEffect } from 'react';

import GeoJSON from 'ol/format/GeoJSON';
import Feature from 'ol/Feature';

import pointsWithPylogon from '@turf/points-within-polygon';
import bboxPolygon from '@turf/bbox-polygon';
import booleanContains from '@turf/boolean-contains';
import transformScale from '@turf/transform-scale';
import axios from 'axios';
import MapWrapper from './MapWrapper';

//const ApiHost = 'stressapi-dev.azurewebsites.net';
const ApiHost = 'localhost:5001';
const RecordUrl = 'https://' + ApiHost + '/api/StressGeoJson/';
const PlateUrl = 'https://' + ApiHost + '/api/PlateGeoJson/';
var oldBounds;
var plateLayer;
var platenameWindow;
var stressLayer;

const updateMap = (map, stresses, plates) => {
    const bounds = map.getBounds().toJSON();
    const bounds_polygon = bboxPolygon([bounds.west, bounds.south, bounds.east, bounds.north]);
    const visible_stresses = pointsWithPylogon(stresses, bounds_polygon);

    stressLayer.addGeoJson(visible_stresses);
};

const handleApiLoaded = (map, maps, stresses, plates) => {
    plateLayer = new maps.Data({map: map});
    stressLayer = new maps.Data({ map: map });
    platenameWindow = new maps.InfoWindow();
    plateLayer.addListener('click', function (event) {
        console.log('plate click');
        platenameWindow.setContent(event.feature.getProperty('name'));
        platenameWindow.setPosition(event.latLng);
        platenameWindow.open(map);
    });
    stressLayer.addListener('click', function (event) {
        console.log('stress click');
    });

    stressLayer.setStyle(function (feature) {
        return {
            icon: {
                url: 'https://' + ApiHost + '/Icons/' + feature.getProperty('icon') + '.png?angle=' + 5*Math.round(feature.getProperty('azimuth')/5),
                scaledSize: new maps.Size(32, 32),
                anchor: new maps.Point(16, 16)
            }
        }
    });

    plateLayer.setStyle(function (feature) {
        return {
            strokeWeight: 2,
            fillOpacity: 0
        }
    });

    const ob = map.getBounds().toJSON();
    oldBounds = bboxPolygon([ob.west, ob.south, ob.east, ob.north]);
    maps.event.addListener(map, 'bounds_changed', () => {
        const nb = map.getBounds().toJSON();
        const newBounds = bboxPolygon([nb.west, nb.south, nb.east, nb.north])   ;

        const oldBox = transformScale(oldBounds, 1.2);
       
        if (!booleanContains(oldBox, newBounds)) {
            console.log('updating map data');
            updateMap(map, stresses, plates);
            oldBounds = newBounds;
        }
    })

    plateLayer.addGeoJson(plates);
    updateMap(map, stresses, plates);
};

export function StressMap(props) {
    const [stresses, setStresses] = useState([]);
    const [plates, setPlates] = useState([]);

    useEffect(() => {
        fetch(RecordUrl)
            .then(response => response.json())
            .then((fetchedFeatures) => {
                const wktOptions = {
                    dataProjection: 'EPSG:4326',
                    featureProjection: 'EPSG:3857'
                }
                const parsedFeatures = new GeoJSON().readFeatures(fetchedFeatures, wktOptions);

                setStresses(parsedFeatures);
        })
    },[]);

    useEffect(() => {
        fetch(PlateUrl)
            .then(response => response.json())
            .then((fetchedFeatures) => {
                const wktOptions = {
                    dataProjection: 'EPSG:4326',
                    featureProjection: 'EPSG:3857'
                }
                const parsedFeatures = new GeoJSON().readFeatures(fetchedFeatures, wktOptions);

                    setPlates(parsedFeatures);
            })
    }, []);

    return (
        <MapWrapper stresses={stresses} plates={[]} />
    );
}
