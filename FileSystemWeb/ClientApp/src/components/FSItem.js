import React from 'react';

function getImagePath(props) {
    return props.isFile ? '/assets/fileIcon.svg' : '/assets/folderIcon.svg';
}

export default function (props) {
    const image = getImagePath(props);

    return (
        <span>
            <img src={image} width="30" />
            <label>
                {props.name}
            </label>
        </span>
    );
}