import {ObservableObject} from './ObservableObject';

let store;
if (!store) store = new ObservableObject();

export default store;