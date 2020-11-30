import React, { Component } from 'react';
import GoogleMapReact from 'google-map-react';
import pointsWithPylogon from '@turf/points-within-polygon';
import bboxPolygon from '@turf/bbox-polygon';
import booleanContains from '@turf/boolean-contains';
import transformScale from '@turf/transform-scale';
import axios from 'axios';

const ApiHost = 'stressapi-dev.azurewebsites.net';
const ApiUrl = 'https://' + ApiHost + '/api/StressGeoJson/';
var oldBounds;

const updateMap = (map, maps, stresses) => {
    const bounds = map.getBounds().toJSON();
    const bounds_polygon = bboxPolygon([bounds.west, bounds.south, bounds.east, bounds.north]);
    const visible_stresses = pointsWithPylogon(stresses, bounds_polygon);

    map.data.addGeoJson(visible_stresses);
};

const handleApiLoaded = (map, maps, stresses) => {
    map.data.setStyle(function (feature) {
        return {
            icon: {
                url: 'https://' + ApiHost + '/Icons/' + feature.getProperty('icon') + '.png?angle=' + 5*Math.round(feature.getProperty('azimuth')/5),
                scaledSize: new maps.Size(32, 32),
                anchor: new maps.Point(16, 16)
            }
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
            updateMap(map, maps, stresses);
            oldBounds = newBounds;
        }
    })

    updateMap(map, maps, stresses);
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
        isLoaded: false,
        stresses: [],
        error: null
    };

    componentDidMount() {
        axios.get(ApiUrl)
            .then(
                (res) => {
                    this.setState({
                        isLoaded: true,
                        stresses: res.data
                    });
                },
                (error) => {
                    this.setState({
                        isLoaded: true,
                        error
                    });
                }
            );

    }

    render() {
        const { isLoaded, stresses, error } = this.state;
        if (error) {
            return <div>Error: {error.message}</div>;
        } else if (!isLoaded) {
            return <div>Loading...</div>;
        } else {
            return (
                <div style={{ height: '100vh', widht: '100%' }}>
                    <GoogleMapReact
                        bootstrapURLKeys={{ key: 'AIzaSyC5-5VL3QXjGSo6D-XD9nju-aXdB1FMuxo' }}
                        defaultCenter={this.props.center}
                        defaultZoom={this.props.zoom}
                        yesIWantToUseGoogleMapApiInternals
                        onGoogleApiLoaded={({ map, maps }) => handleApiLoaded(map, maps, stresses)}
                    >
                    </GoogleMapReact>
                </div>
            );
        }
    }
}
