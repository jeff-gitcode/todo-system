"use client";

import { useRouter } from "next/navigation";

export default function Home() {
  const router = useRouter();

  return (
    <div>Home</div>
    // <div className="flex flex-row gap-4 justify-center items-center h-screen">
    //   <button
    //     className="bg-white text-black py-3 px-12 rounded-full cursor-pointer hover:bg-neutral-300"
    //     onClick={() => router.push("/signup")}>
    //     Sign Up
    //   </button>
    //   <button
    //     className="border-2 border-white text-white py-3 px-12 rounded-full cursor-pointer hover:bg-neutral-900"
    //     onClick={() => router.push("/login")}>
    //     Sign In
    //   </button>
    // </div>
  );
}