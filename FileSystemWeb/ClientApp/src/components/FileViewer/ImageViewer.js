import React, {Component} from 'react';
import Loading from "../Loading/Loading";
import {encodeBase64UnicodeCustom} from "../../Helpers/Path";
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
        const imageUrl = `/api/files/${encodeBase64UnicodeCustom(this.props.path)}`;

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