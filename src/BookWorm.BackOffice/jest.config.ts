import nextJest from "next/jest.js"
import type { Config } from "jest"

const createJestConfig = nextJest({ dir: "./" })

const config: Config = { testEnvironment: "node" }

// createJestConfig is exported this way to ensure that next/jest can load the Next.js config which is async
export default createJestConfig(config)
