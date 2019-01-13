import React, { Component } from 'react';
import logo from './logo.svg';
import 'ol/ol.css';
import './App.css';

// import {
//     interaction, layer, custom, control, //name spaces
//     Interactions, Overlays, Controls,     //group
//     Map, Layers, Overlay, Util    //objects
// }
//     from "react-openlayers--ajp-typed";

import Attribution from 'ol/control/Attribution';
import { defaults, ZoomToExtent, ScaleLine, FullScreen } from 'ol/control';
import MousePosition from 'ol/control/MousePosition';
import { boundingExtent } from 'ol/extent';
import TileLayer from 'ol/layer/Tile';
import { Projection } from 'ol/proj';
import { XYZ } from 'ol/source';
import TileGrid from 'ol/tilegrid/TileGrid';
import { Map, View } from 'ol';

const DEFAULT_EXTENT = boundingExtent([[-12550824, -12550824], [12550824, 12550824]]);
const projection = new Projection({
    code: 'mcbe-mapper',
    units: 'pixels',
    axisOrientation: 'esu',
    extent: DEFAULT_EXTENT,
    getPointResolution: (r, _c) => r,

});

const BIOMES_SOURCE = new XYZ({
    attributions: new Attribution({ html: 'Created by mc-mapper, a .NET Core project for rendering Minecraft worlds.' }),
    //url: 'http://localhost:7887/biome/tile1024/?dim=0&tileX={x}&tileZ={y}',
    tileUrlFunction: (coord, _ratio, _projection) => {
        const x = coord[1];
        const y = -coord[2];
        return `http://localhost:7887/biome/tile1024/?dim=0&tileX=${x - 12256}&tileZ=${y - 12256}`;
    },
    projection,
    tileSize: [1024, 1024],
    tileGrid: new TileGrid({
        extent: DEFAULT_EXTENT,
        minZoom: 0,
        maxZoom: 1,
        tileSize: [1024, 1024],
        resolutions: [1], // todo: 16
    }),
});

const BIOMES_LAYER = new TileLayer({
    extent: DEFAULT_EXTENT,
    source: BIOMES_SOURCE,
});

class App extends Component {
    mapDiv;
    map;
    componentDidMount() {
        this.map = new Map({
            controls: new defaults({
                attribution: true,
                attributionOptions: { collapsed: false, collapsible: false, target: '_blank' }
            }).extend([
                    new ZoomToExtent(),
                    new ScaleLine(),
                    new FullScreen(),
                    new MousePosition(),
                ]),
            target: this.mapDiv,
            view: new View({
                projection,
                center: [0, 0],
                resolution: 1,
            }),
            layers: [BIOMES_LAYER],
        });
    }
    render() {
        return (
            <div className="App">
                <div style={{ width: '100%', height: '100%', backgroundColor: 'black' }} ref={div => this.mapDiv = div}>

                </div>
            </div>
        );
    }
}

export default App;
