// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract QuizManager {
    address public owner;
    uint256 public constant ENTRY_FEE = 1 ether;

    struct QuizPayment {
        bool paid;
        uint256 timestamp;
    }

    // Mapping: user address => quizId => Payment status
    mapping(address => mapping(uint256 => QuizPayment)) public payments;
    
    // Mapping: user address => quizId => score
    mapping(address => mapping(uint256 => uint8)) public scores;

    event PaymentReceived(address indexed user, uint256 indexed quizId, uint256 amount);
    event ScoreRecorded(address indexed user, uint256 indexed quizId, uint8 score);

    constructor() {
        owner = msg.sender;
    }

    function payForQuiz(uint256 quizId) external payable {
        require(msg.value == ENTRY_FEE, "Must pay exactly 1 ETH to enter the quiz");
        require(!payments[msg.sender][quizId].paid, "Already paid for this quiz");

        payments[msg.sender][quizId] = QuizPayment({
            paid: true,
            timestamp: block.timestamp
        });

        emit PaymentReceived(msg.sender, quizId, msg.value);
    }

    function recordResult(uint256 quizId, uint8 score) external {
        require(payments[msg.sender][quizId].paid, "You must pay to record your result");
        
        scores[msg.sender][quizId] = score;
        emit ScoreRecorded(msg.sender, quizId, score);
    }

    function hasPaid(address user, uint256 quizId) external view returns (bool) {
        return payments[user][quizId].paid;
    }

    function getScore(address user, uint256 quizId) external view returns (uint8) {
        return scores[user][quizId];
    }

    function withdraw() external {
        require(msg.sender == owner, "Only owner can withdraw");
        payable(owner).transfer(address(this).balance);
    }
}
