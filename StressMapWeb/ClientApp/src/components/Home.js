import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

    render() {
        return (
            <div>
                <h1>Stress Map</h1>
                <p>The Map view uses Google Maps to overlay data from various sources:</p>
                <ul>
                    <li><a href='http://www.world-stress-map.org/'>World Stress Map</a> is used for the stress markers</li>
                    <li><a href='http://peterbird.name/publications/2003_PB2002/2003_PB2002.htm'>An updated digital model of plate boundaries</a> by Peter Bird in Geochemistry Geophysics Geosystems is used for plate boundaries</li>
                </ul>
            </div>
        );
    }
}
