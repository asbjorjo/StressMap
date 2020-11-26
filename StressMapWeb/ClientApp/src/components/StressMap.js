import React, { Component } from 'react';
import GoogleMapReact from 'google-map-react';
import axios from 'axios';

const AnyReactComponent = ({ text }) => <div>{text}</div>;
const ApiUrl = 'https://localhost:5001/api/StressGeoJson';

const handleApiLoaded = (map, maps, stresses) => {
    map.data.addGeoJson(stresses);
};

export class StressMap extends Component {
    static displayName = StressMap.name;

    static defaultProps = {
        center: {
            lat: 0,
            lng: 0
        },
        zoom: 2
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
                        <AnyReactComponent
                            lat={59.955413}
                            lng={30.337844}
                            text="My Marker"
                        />
                    </GoogleMapReact>
                </div>
            );
        }
    }
}
