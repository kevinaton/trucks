import * as React from 'react';
import { GoogleMap, MarkerF, useLoadScript } from '@react-google-maps/api';

const mapContainerStyle = {
    width: '100%',
    height: '600px',
    borderRadius: '3px',
};

const onLoad = (marker) => {
    console.log('marker: ', marker);
};

const MapView = ({ markers, center }) => {
    const { isLoaded } = useLoadScript({
        googleMapsApiKey: 'AIzaSyDKr-CkJW-c1bcmttDCg75s3RCKobcLwOk',
    });

    return isLoaded ? (
        <GoogleMap id='truck-map' mapContainerStyle={mapContainerStyle} center={center} zoom={12}>
            {markers.map((marker, index) => (
                <MarkerF key={index} onLoad={onLoad} position={marker} />
            ))}
        </GoogleMap>
    ) : (
        <></>
    );
};

export default MapView;
