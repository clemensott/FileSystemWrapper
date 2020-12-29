export class ObservableObject {
    constructor() {
        this.dict = {};
        this.callbacks = {};
    }

    get(key) {
        return this.dict[key];
    }

    set(key, value) {
        const oldValue = this.dict[key];
        this.dict[key] = value;

        if (this.callbacks[key]) {
            Object.values(this.callbacks[key])
                .forEach(callback => typeof callback === 'function' && callback(value, oldValue, key));
        }
    }

    addCallback(key, callback) {
        if (!this.callbacks[key]) this.callbacks[key] = {};

        let id;
        do {
            id = Math.round(Math.random() * 1000000) + 1;
        } while (this.callbacks[key][id]);

        this.callbacks[key][id] = callback;
        return id;
    }

    removeCallback(key, id) {
        delete this.callbacks[key][id];
    }
}