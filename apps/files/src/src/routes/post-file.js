const { Request, Response } = require("express");

const postFile = (_) => {
    /**
     * @param {Request} req
     * @param {Response} res
     */
    return (req, res) => {
        let file = req.file;

        res.status(201).send({
            data: file
        })
    }
}

module.exports = { postFile }
