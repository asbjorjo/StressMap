import React, { Component } from 'react';
import GoogleMapReact from 'google-map-react';
import pointsWithPylogon from '@turf/points-within-polygon';
import bboxPolygon from '@turf/bbox-polygon';
import booleanContains from '@turf/boolean-contains';
import transformScale from '@turf/transform-scale';
import rewind from '@turf/rewind';
import axios from 'axios';

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

export class StressMap extends Component {
    static displayName = StressMap.name;

    static defaultProps = {
        center: {
            lat: 51.17,
            lng: 10.45
        },
        zoom: 5
    };

    state = {
        stressesLoaded: false,
        platesLoaded: false,
        stresses: [],
        plates: [],
        error: null
    };

    componentDidMount() {
        axios.get(RecordUrl)
            .then(
                (res) => {
                    this.setState({
                        stressesLoaded: true,
                        stresses: res.data
                    });
                },
                (error) => {
                    this.setState({
                        stressesLoaded: true,
                        error
                    });
                }
            );
        axios.get(PlateUrl)
            .then(
                (res) => {
                    //var jsonPlates = res.data;
                    this.setState({
                        platesLoaded: true,
                        plates: res.data
                    });
                },
                (error) => {
                    this.setState({
                        platesLoaded: true,
                        error
                    });
                }
            );
    }

    render() {
        const { stressesLoaded, platesLoaded, stresses, plates, error } = this.state;
        if (error) {
            return <div>Error: {error.message}</div>;
        } else if (!stressesLoaded || !platesLoaded) {
            return <div>Loading...</div>;
        } else {
            return (
                <div style={{ height: '100vh', widht: '100%' }}>
                    <GoogleMapReact
                        bootstrapURLKeys={{ key: 'AIzaSyC5-5VL3QXjGSo6D-XD9nju-aXdB1FMuxo' }}
                        defaultCenter={this.props.center}
                        defaultZoom={this.props.zoom}
                        yesIWantToUseGoogleMapApiInternals
                        onGoogleApiLoaded={({ map, maps }) => handleApiLoaded(map, maps, stresses, plates)}
                    >
                    </GoogleMapReact>
                </div>
            );
        }
    }
}
