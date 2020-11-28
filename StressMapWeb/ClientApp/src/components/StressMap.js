import React, { Component } from 'react';
import GoogleMapReact from 'google-map-react';
import pointsWithPylogon from '@turf/points-within-polygon';
import bboxPolygon from '@turf/bbox-polygon';
import axios from 'axios';

const ApiUrl = 'https://localhost:5001/api/StressGeoJson';

const handleApiLoaded = (map, maps, stresses) => {
    map.data.setStyle(function (feature) {
        return {
            icon: {
                url: 'https://localhost:5001/Icons/' + feature.getProperty('icon') + '.png?angle=' + feature.getProperty('azimuth'),
                scaledSize: new maps.Size(32, 32),
                anchor: new maps.Point(16, 16)
            }
        }
    });
    const bounds = map.getBounds().toJSON();
    console.log(bounds);
    const bounds_polygon = bboxPolygon([bounds.west, bounds.south, bounds.east, bounds.north]);
    const visible_stresses = pointsWithPylogon(stresses, bounds_polygon);

    map.data.addGeoJson(visible_stresses);
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
