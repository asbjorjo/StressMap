// react
import React, { useState, useEffect, useRef } from 'react';

// openlayers
import Map from 'ol/Map'
import View from 'ol/View'
import TileLayer from 'ol/layer/Tile'
import VectorLayer from 'ol/layer/Vector'
import VectorImageLayer from 'ol/layer/VectorImage'
import GeoJSON from 'ol/format/GeoJSON';
import OSM from 'ol/source/OSM'
import VectorSource from 'ol/source/Vector'
import XYZ from 'ol/source/XYZ'
import { transform } from 'ol/proj'
import { toStringXY } from 'ol/coordinate'
import { Icon, Style } from 'ol/style'
import { Attribution, defaults as defaultControls } from 'ol/control'

function MapWrapper(props) {
    //const ApiHost = 'stressapi-dev.azurewebsites.net';
    const ApiHost = 'localhost:5001';
    const RecordUrl = 'https://' + ApiHost + '/api/StressGeoJson/';
    const PlateUrl = 'https://' + ApiHost + '/api/PlateGeoJson/';

    // set intial state
    const [map, setMap] = useState()
    const [stressLayer, setStressLayer] = useState()
    const [platesLayer, setPlatesLayer] = useState()
    const [selectedCoord, setSelectedCoord] = useState()

    // pull refs
    const mapElement = useRef()

    // create state ref that can be accessed in OpenLayers onclick callback function
    //  https://stackoverflow.com/a/60643670
    const mapRef = useRef()
    mapRef.current = map

    var stressStyles = {}

    // initialize map on first render - logic formerly put into componentDidMount
    useEffect(() => {

        // create and add vector source layer
        const stressLayer = new VectorLayer({
            source: new VectorSource(),
            style: function (feature) {
                const angle = 5 * Math.round(feature.get('azimuth') / 5)

                const styleName = feature.get('icon') + '_' + angle

                if (!stressStyles[styleName]) {
                    stressStyles[styleName] = new Style({
                        image: new Icon({
                            anchor: [0.5, 0.5],
                            anchorXUnits: "fraction",
                            scale: 0.5,
                            src: 'https://' + ApiHost + '/Icons/' + feature.get('icon') + '.png?angle=' + angle
                        }),
                    });
                }
                
                return stressStyles[styleName]
            },
        })

        const platesLayer = new VectorLayer({
            source: new VectorSource(),
        })

        // create map
        const initialMap = new Map({
            target: mapElement.current,
            layers: [
                // OSM
                new TileLayer({
                    source: new OSM(),
                }),

                // USGS Topo
                /*new TileLayer({
                    source: new XYZ({
                        url: 'https://basemap.nationalmap.gov/arcgis/rest/services/USGSTopo/MapServer/tile/{z}/{y}/{x}',
                    })
                }), */

                // Google Maps Terrain
                /* new TileLayer({
                  source: new XYZ({
                    url: 'http://mt0.google.com/vt/lyrs=p&hl=en&x={x}&y={y}&z={z}',
                  })
                }), */

                stressLayer,
                platesLayer,
            ],
            view: new View({
                projection: 'EPSG:3857',
                center: [0, 0],
                zoom: 5
            }),
            controls: defaultControls({ attribution: false }).extend(new Attribution({ collapsible: false })),
        })

        // set map onclick handler
        //initialMap.on('click', handleMapClick)

        // save map and vector layer references to state
        setMap(initialMap)
        setStressLayer(stressLayer)
        setPlatesLayer(platesLayer)

    }, [])

    // update map if stresses prop changes - logic formerly put into componentDidUpdate
    useEffect(() => {

        if (props.stresses.length) { // may be null on first render

            // set features to map
            stressLayer.setSource(
                new VectorSource({
                    features: props.stresses, // make sure features is an array
                })
            )

            // fit map to feature extent (with 100px of padding)
            map.getView().fit(stressLayer.getSource().getExtent(), {
                padding: [100, 100, 100, 100]
            })

        }

    }, [props.stresses])

    useEffect(() => {

        if (props.plates.length) { // may be null on first render

            // set features to map
            platesLayer.setSource(
                new VectorSource({
                    features: props.plates, // make sure features is an array
                    wrapX: true,
                })
            )

            // fit map to feature extent (with 100px of padding)
            /*map.getView().fit(stressLayer.getSource().getExtent(), {
                padding: [100, 100, 100, 100]
            })*/

        }

    }, [props.plates])


    // map click handler
    const handleMapClick = (event) => {

        // get clicked coordinate using mapRef to access current React state inside OpenLayers callback
        //  https://stackoverflow.com/a/60643670
        const clickedCoord = mapRef.current.getCoordinateFromPixel(event.pixel);

        // transform coord to EPSG 4326 standard Lat Long
        const transormedCoord = transform(clickedCoord, 'EPSG:3857', 'EPSG:4326')

        // set React state
        setSelectedCoord(transormedCoord)

    }

    // render component
    return (
        <div>

            <div ref={mapElement} className="map-container"></div>

            <div className="clicked-coord-label">
`               <p>{(selectedCoord) ? toStringXY(selectedCoord, 5) : ''}</p>
            </div>

        </div>
    )

}

export default MapWrapper