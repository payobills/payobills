const http = require('http')
const { Readable } = require('stream');
const FormData = require('form-data');
const axios = require('axios')

module.exports = class NocoDbClient {
    /**
     * Initializes a new instance of NocoDbClient.
     * @param {Object} opts The options object which should be an object with the following shape:
     * @param {string} opts.baseUrl URL of the NocoDB instance
     * @param {string} opts.xcToken Token to use for authentication
     * @param {string} opts.projectId Name of the NocoDb Project (base)
     * @param {string} opts.tableId Name of the NocoDb Table inside the Project specified
     */
    constructor(opts) {
        this.opts = opts;
    }

    /**
     * Returns the base URL of the NocoDB instance.
     * @returns {string} The base URL of the NocoDB instance.
     */
    getBaseUrl() {
        return this.opts.baseUrl;
    }

    async getRecord(projectId, tableId, id) {
        if (!projectId || !tableId || !id) {
            throw new Error("Project ID, Table ID and Record ID are required to fetch a record.");
        }

        const url = `${this.opts.baseUrl}/api/v1/db/data/nc/${projectId}/${tableId}/${id}?fields=*`;
        console.log(`Fetching record from NocoDB: URL=${url}`);
        const res = await axios({
            method: 'get',
            url,
            headers: {
                'xc-token': this.opts.xcToken,
            },
        });
        if (res.status !== 200) {
            throw new Error(`Failed to fetch record: ${res.status} ${res.statusText}`);
        }

        return res.data;
    }

    async patchRecord(
        row,
        id
    ) {
        let url = `${this.opts.baseUrl}/api/v1/db/data/nc/${this.opts.projectId}/${this.opts.tableId}/${id}`;
        console.log(`Updating record from NocoDB: URL=${url}`);

        let rowUploadResponse = await axios.request({
            url,
            method: 'PATCH',
            data: row,
            headers: {
                'Content-Type': "application/json",
                'xc-token': this.opts.xcToken
            }
        })

        if (![200, 201].includes(rowUploadResponse.status)) {
            throw new Error(`Row update failed with status ${rowUploadResponse.status}: ${rowUploadResponse.data}`);
        }

        var nocodbRow = await rowUploadResponse.data;
        return nocodbRow;
    }

    async putObject(
        _,
        fileName,
        buffer,
        tags
    ) {
        let formData = new FormData();
        formData.append('file', Readable.from(buffer), {
            filename: fileName,
        });

        let config = {
            method: 'post',
            maxBodyLength: Infinity,
            url: `${this.opts.baseUrl}/api/v1/db/storage/upload`,
            headers: {
                'Content-Type': 'multipart/form-data',
                'xc-token': this.opts.xcToken,
            },
            data: formData,
        };

        // Using Axios for File Upload
        // TODO: Use fetch - fetch isn't working for this, needs investigation
        let response = await axios.request(config)

        let row = {
            "Files": response.data,
            Tags: tags
        };

        let url = `${this.opts.baseUrl}/api/v1/db/data/nc/${this.opts.projectId}/${this.opts.tableId}`;

        let rowUploadResponse = await fetch(url, {
            method: 'POST',
            body: JSON.stringify(row),
            headers: {
                'Content-Type': "application/json",
                'xc-token': this.opts.xcToken
            }
        })

        if (![200, 201].includes(rowUploadResponse.status)) {
            throw new Error(`Row upload failed with status ${rowUploadResponse.status}: ${rowUploadResponse.statusText}`);
        }

        var nocodbRow = await rowUploadResponse.json();
        return { etag: nocodbRow.Id.toString() }
    }
}
