const QuizManager = artifacts.require("QuizManager");

module.exports = function (deployer) {
    deployer.deploy(QuizManager);
};
