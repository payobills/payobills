const http = require('http')
const { Readable } = require('stream');
const FormData = require('form-data');
const fs = require('fs');
const axios = require('axios')

module.exports = class NocoDbClient {
    /**
     * Initializes a new instance of NocoDbClient.
     * @param {Object} opts The options object which should be an object with the following shape:
     * @param {string} opts.url URL of the NocoDB instance
     * @param {string} opts.xcToken Token to use for authentication
     * @param {string} opts.projectId Name of the NocoDb Project (base)
     * @param {string} opts.tableId Name of the NocoDb Table inside the Project specified
     */
    constructor(opts) {
        this.opts = opts;
    }

    async putObject(
        projectId,
        fileName,
        buffer,
        tags
    ) {

        var baseUrl = 'http://localhost:8080'

        let formData = new FormData();
        formData.append('file', fs.ReadStream.from(buffer), {
            filename: fileName,
        });

        let config = {
            method: 'post',
            maxBodyLength: Infinity,
            url: `${baseUrl}/api/v1/db/storage/upload`,
            headers: {
                'Content-Type': 'multipart/form-data',
                'xc-token': this.opts.xcToken,
            },
            data: formData,
        };

        // Using Axios for File Upload
        // TODO: Use fetch - fetch isn't working for this, needs investigation
        var response = await axios.request(config)

        let row = {
            "Name": fileName,
            "File": response.data,
            "Tags": tags
        };

        try {
            await fetch(`${baseUrl}/api/v1/db/data/nc/${this.opts.projectId}/${this.opts.tableId}`, {
                method: 'POST',
                body: JSON.stringify(row),
                headers: {
                    'Content-Type': "application/json",
                    'xc-token': this.opts.xcToken
                }
            })
        } catch (e) {
            console.error(e);
        }
    }
}
