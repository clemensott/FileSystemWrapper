import React, {Component} from 'react';
import {formatUrl} from '../../Helpers/Fetch';
import Loading from "../Loading/Loading";
import './ImageViewer.css'

export default class ImageViewer extends Component {
    static displayName = ImageViewer.name;

    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            error: null,
        };
    }

    render() {
        const imageUrl = formatUrl({
            resource: '/api/files',
            path: this.props.path,
        });

        return (
            <div className="image-container">
                <img src={imageUrl} className={`image-content ${this.state.error ? 'd-none' : ''}`}
                     onLoad={() => this.setState({isLoading: false,})}
                     onError={() => this.setState({error: true})}/>

                <div className={this.state.isLoading ? 'center' : 'd-none'}>
                    <Loading/>
                </div>
            </div>
        );
    }
}