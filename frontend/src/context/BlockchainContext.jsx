import React, { createContext, useContext, useState, useEffect } from 'react';
import { ethers } from 'ethers';

const BlockchainContext = createContext();

export const useBlockchain = () => useContext(BlockchainContext);

const CONTRACT_ADDRESS = "0x88BfCbfE55D5648701A927A50511FD2F80e56812";
const ABI = [
    "function payForQuiz(uint256 quizId) external payable",
    "function hasPaid(address user, uint256 quizId) external view returns (bool)",
    "function recordResult(uint256 quizId, uint8 score) external",
    "function getScore(address user, uint256 quizId) external view returns (uint8)",
    "function ENTRY_FEE() external view returns (uint256)"
];

export const BlockchainProvider = ({ children }) => {
    const [account, setAccount] = useState(null);
    const [provider, setProvider] = useState(null);
    const [contract, setContract] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const connectWallet = async () => {
        if (!window.ethereum) {
            setError("MetaMask not found. Please install MetaMask.");
            return;
        }

        try {
            setLoading(true);
            const accounts = await window.ethereum.request({ method: 'eth_requestAccounts' });
            setAccount(accounts[0]);

            const tempProvider = new ethers.BrowserProvider(window.ethereum);
            const signer = await tempProvider.getSigner();
            const tempContract = new ethers.Contract(CONTRACT_ADDRESS, ABI, signer);

            setProvider(tempProvider);
            setContract(tempContract);
            setError(null);
        } catch (err) {
            console.error("Connection error:", err);
            setError("Failed to connect wallet.");
        } finally {
            setLoading(false);
        }
    };

    const checkPaymentStatus = async (quizId) => {
        if (!contract || !account) return false;
        try {
            return await contract.hasPaid(account, quizId);
        } catch (err) {
            console.error("Check payment error:", err);
            return false;
        }
    };

    const payForQuiz = async (quizId) => {
        if (!contract) return false;
        try {
            setLoading(true);
            const fee = ethers.parseEther("1.0");
            const tx = await contract.payForQuiz(quizId, { value: fee });
            await tx.wait();
            return true;
        } catch (err) {
            console.error("Payment error:", err);
            setError("Payment failed. Make sure you have enough ETH.");
            return false;
        } finally {
            setLoading(false);
        }
    };

    const recordResult = async (quizId, score) => {
        if (!contract) return;
        try {
            setLoading(true);
            const tx = await contract.recordResult(quizId, score);
            await tx.wait();
        } catch (err) {
            console.error("Record result error:", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (window.ethereum) {
            window.ethereum.on('accountsChanged', (accounts) => {
                setAccount(accounts[0] || null);
            });
        }
    }, []);

    return (
        <BlockchainContext.Provider value={{
            account,
            loading,
            error,
            connectWallet,
            checkPaymentStatus,
            payForQuiz,
            recordResult
        }}>
            {children}
        </BlockchainContext.Provider>
    );
};
