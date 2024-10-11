const http = require('http')
const { Readable } = require('stream');
const FormData = require('form-data');
const fs = require('fs');
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

    async putObject(
        _,
        fileName,
        buffer,
        tags
    ) {
        let formData = new FormData();
        formData.append('file', fs.ReadStream.from(buffer), {
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
            "CorrelationID": tags['correlationID'],
            "Files": response.data,
            Tags: tags
        };

        let rowUploadResponse = await fetch(`${this.opts.baseUrl}/api/v1/db/data/nc/${this.opts.projectId}/${this.opts.tableId}`, {
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
    }
}
